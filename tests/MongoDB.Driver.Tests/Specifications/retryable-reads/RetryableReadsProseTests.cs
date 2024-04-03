﻿/* Copyright 2021-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers;
using MongoDB.Driver.Core.TestHelpers.Logging;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.TestHelpers;
using MongoDB.Driver.Tests.Specifications.connection_monitoring_and_pooling;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;
using Xunit.Abstractions;

namespace MongoDB.Driver.Tests.Specifications.retryable_reads
{
    public class RetryableReadsProseTests : LoggableTestClass
    {
        public RetryableReadsProseTests(ITestOutputHelper output) : base(output, includeAllCategories: true)
        {
        }

        [Theory]
        [ParameterAttributeData]
        public async Task PoolClearedError_read_retryablity_test([Values(true, false)] bool async)
        {
            RequireServer.Check().Supports(Feature.FailPointsBlockConnection);

            var heartbeatInterval = TimeSpan.FromMilliseconds(50);
            var eventsWaitTimeout = TimeSpan.FromMilliseconds(5000);

            var failPointCommand = BsonDocument.Parse(
                $@"{{
                    configureFailPoint : 'failCommand',
                    mode : {{ 'times' : 1 }},
                    data :
                    {{
                        failCommands : [ 'find' ],
                        errorCode : 91,
                        blockConnection: true,
                        blockTimeMS: 1000
                    }}
                }}");

            IServerSelector failPointSelector = new ReadPreferenceServerSelector(ReadPreference.Primary);
            var settings = DriverTestConfiguration.GetClientSettings();

            if (CoreTestConfiguration.Cluster.Description.Type == ClusterType.Sharded)
            {
                var serverAddress = settings.Servers.First();
                settings.Servers = new[] { serverAddress };

                // set settings.DirectConnection = true after removing obsolete ConnectionMode
#pragma warning disable CS0618 // Type or member is obsolete
                settings.ConnectionMode = ConnectionMode.Direct;
#pragma warning restore CS0618 // Type or member is obsolete

                failPointSelector = new EndPointServerSelector(new DnsEndPoint(serverAddress.Host, serverAddress.Port));
            }

            settings.MaxConnectionPoolSize = 1;
            settings.RetryReads = true;

            var eventCapturer = new EventCapturer()
               .Capture<ConnectionPoolClearedEvent>()
               .Capture<ConnectionPoolCheckedOutConnectionEvent>()
               .Capture<ConnectionPoolCheckingOutConnectionFailedEvent>()
               .CaptureCommandEvents("find");

            var failpointServer = DriverTestConfiguration.Client.Cluster.SelectServer(failPointSelector, default);
            using var failPoint = FailPoint.Configure(failpointServer, NoCoreSession.NewHandle(), failPointCommand);

            using var client = CreateClient(settings, eventCapturer, heartbeatInterval);
            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<BsonDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);

            eventCapturer.Clear();

            if (async)
            {
                await ThreadingUtilities.ExecuteTasksOnNewThreads(2, async __ =>
                {
                    var cursor = await collection.FindAsync(FilterDefinition<BsonDocument>.Empty);
                    _ = await cursor.ToListAsync();
                });
            }
            else
            {
                ThreadingUtilities.ExecuteOnNewThreads(2, __ =>
                {
                    _ = collection.Find(FilterDefinition<BsonDocument>.Empty).ToList();
                });
            }

            // wait for 2 CommandSucceededEvent events, meaning that all other events should be received
            eventCapturer.WaitForOrThrowIfTimeout(
                events => events.OfType<CommandSucceededEvent>().Count() == 2,
                eventsWaitTimeout);

            eventCapturer.Events.OfType<CommandStartedEvent>().Count().Should().Be(3);
            eventCapturer.Events.OfType<CommandFailedEvent >().Count().Should().Be(1);
            eventCapturer.Events.OfType<CommandSucceededEvent>().Count().Should().Be(2);
            eventCapturer.Events.OfType<ConnectionPoolClearedEvent>().Count().Should().Be(1);
            eventCapturer.Events.OfType<ConnectionPoolCheckedOutConnectionEvent>().Count().Should().Be(3);
            eventCapturer.Events.OfType<ConnectionPoolCheckingOutConnectionFailedEvent>().Count().Should().Be(1);
        }

        [Fact]
        public void Sharded_cluster_retryable_reads_are_retried_on_different_mongos_if_available()
        {
            RequireServer.Check()
                .Supports(Feature.FailPointsFailCommandForSharded)
                .ClusterTypes(ClusterType.Sharded)
                .MultipleMongoses(true);

            var failPointCommand = BsonDocument.Parse(
                @"{
                    configureFailPoint: ""failCommand"",
                    mode: { times: 1 },
                    data:
                    {
                        failCommands: [""find""],
                        errorCode: 6
                    }
                }");

            var eventCapturer = new EventCapturer()
                .Capture<CommandFailedEvent>();

            using var client = DriverTestConfiguration.CreateDisposableClient(
                s =>
                {
                    s.RetryReads = true;
                    s.ClusterConfigurator = b => b.Subscribe(eventCapturer);
                }
                , LoggingSettings, true);

            var failPointServer1 = client.Cluster.SelectServer(new EndPointServerSelector(client.Cluster.Description.Servers[0].EndPoint), default);
            var failPointServer2 = client.Cluster.SelectServer(new EndPointServerSelector(client.Cluster.Description.Servers[1].EndPoint), default);

            using var failPoint1 = FailPoint.Configure(failPointServer1, NoCoreSession.NewHandle(), failPointCommand);
            using var failPoint2 = FailPoint.Configure(failPointServer2, NoCoreSession.NewHandle(), failPointCommand);

            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<BsonDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);

            Assert.Throws<MongoCommandException>(() =>
            {
                collection.Find(Builders<BsonDocument>.Filter.Empty).ToList();
            });

            eventCapturer.Events.OfType<CommandFailedEvent>().Count().Should().Be(2);

            var event1 = eventCapturer.Events[0].As<CommandFailedEvent>();
            var event2 = eventCapturer.Events[1].As<CommandFailedEvent>();
            event1.CommandName.Should().Be(event2.CommandName).And.Be("find");
            event1.ConnectionId.ServerId().Should().NotBe(event2.ConnectionId.ServerId());

            // Assert the deprioritization debug message was emitted for deprioritized server.
            Logs.Count(log => log.Category == "MongoDB.ServerSelection" && log.Message.StartsWith("Deprioritization")).Should().Be(1);
        }

        [Fact]
        public void Sharded_cluster_retryable_reads_are_retried_on_same_mongo_if_no_other_is_available()
        {
            RequireServer.Check()
                .Supports(Feature.FailPointsFailCommandForSharded)
                .ClusterTypes(ClusterType.Sharded);

            var failPointCommand = BsonDocument.Parse(
                @"{
                    configureFailPoint: ""failCommand"",
                    mode: { times: 1 },
                    data:
                    {
                        failCommands: [""find""],
                        errorCode: 6
                    }
                }");

            var eventCapturer = new EventCapturer()
                .Capture<CommandSucceededEvent>()
                .Capture<CommandFailedEvent>();

            using var client = DriverTestConfiguration.CreateDisposableClient(
                s =>
                {
                    s.RetryReads = true;
                    s.DirectConnection = false;
                    s.ClusterConfigurator = b => b.Subscribe(eventCapturer);
                }
                , LoggingSettings, true);

            var failPointServer = client.Cluster.SelectServer(new EndPointServerSelector(client.Cluster.Description.Servers[0].EndPoint), default);

            using var failPoint = FailPoint.Configure(failPointServer, NoCoreSession.NewHandle(), failPointCommand);

            var database = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            var collection = database.GetCollection<BsonDocument>(DriverTestConfiguration.CollectionNamespace.CollectionName);

            // clear command succeeded events captured from initial hello
            eventCapturer.Clear();

            collection.Find(Builders<BsonDocument>.Filter.Empty).ToList();

            eventCapturer.Events.Count.Should().Be(2);
            eventCapturer.Events.OfType<CommandFailedEvent>().Count().Should().Be(1);
            eventCapturer.Events.OfType<CommandSucceededEvent>().Count().Should().Be(1);

            var event1 = eventCapturer.Events[0].As<CommandFailedEvent>();
            var event2 = eventCapturer.Events[1].As<CommandSucceededEvent>();
            event1.CommandName.Should().Be(event2.CommandName).And.Be("find");
            event1.ConnectionId.ServerId().Should().Be(event2.ConnectionId.ServerId());

            // Assert the deprioritization debug messages were emitted
            // one for deprioritizing the failpointServer and another for
            // reverting the deprioritization since we have no other suitable servers in this test
            Logs.Count(log => log.Category == "MongoDB.ServerSelection" && log.Message.StartsWith("Deprioritization")).Should().Be(2);
        }

        // private methods
        private DisposableMongoClient CreateClient(MongoClientSettings mongoClientSettings, EventCapturer eventCapturer, TimeSpan heartbeatInterval, string applicationName = null)
        {
            var clonedClientSettings = mongoClientSettings ?? DriverTestConfiguration.Client.Settings.Clone();
            clonedClientSettings.ApplicationName = applicationName;
            clonedClientSettings.HeartbeatInterval = heartbeatInterval;
            clonedClientSettings.ClusterConfigurator = builder => builder.Subscribe(eventCapturer);

            return DriverTestConfiguration.CreateDisposableClient(clonedClientSettings);
        }
    }
}
