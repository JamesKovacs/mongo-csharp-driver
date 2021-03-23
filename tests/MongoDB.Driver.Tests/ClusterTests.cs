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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using MongoDB.Driver.Core.TestHelpers;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.TestHelpers;
using Xunit;

namespace MongoDB.Driver.Tests
{
    public class ClusterTests
    {
        private static readonly HashSet<string> __commandsToNotCapture = new HashSet<string>
        {
            "isMaster",
            "buildInfo",
            "getLastError",
            "authenticate",
            "saslStart",
            "saslContinue",
            "getnonce"
        };

        private const string _collectionName = "test";
        private const string _databaseName = "test";

        /// <summary>
        /// Test that starting a new transaction on a pinned ClientSession unpins the
        /// session and normal server selection is performed for the next operation.
        /// </summary>
        [SkippableTheory]
        [ParameterAttributeData]
        public void SelectServer_loadbalancing_prose_test([Values(false, true)] bool async)
        {
            RequireServer.Check().Supports(Feature.ShardedTransactions).ClusterType(ClusterType.Sharded);
            RequireMultipleShardRouters();

            const string applicationName = "loadBalancingTest";
            const int threadsCount = 10;
            const int commandsPerThreadCount = 10;
            const double maxCommandsOnSlowServerRatio = 0.25;
            const double operationsCountTolerance = 0.10;

            var failCommand = BsonDocument.Parse($"{{ configureFailPoint: 'failCommand', mode : {{ times : 1000 }}, data : {{ failCommands : [\"find\"], blockConnection: true, blockTimeMS: 500, appName: '{applicationName}' }} }}");

            DropCollection();
            var eventCapturer = CreateEventCapturer();
            var listOfFindResults = new List<List<BsonDocument>>();
            using (var client = CreateDisposableClient(eventCapturer, applicationName))
            {
                var failPointServer = client.Cluster.SelectServer(WritableServerSelector.Instance, default);
                using var failPoint = FailPoint.Configure(failPointServer, NoCoreSession.NewHandle(), failCommand);

                var database = client.GetDatabase(_databaseName);
                CreateCollection();
                var collection = database.GetCollection<BsonDocument>(_collectionName);

                var (allCount, eventsOnSlowServerCount) = ExecuteFindOperations(collection, failPointServer.ServerId);

                eventsOnSlowServerCount.Should().BeLessThan((int)(allCount * maxCommandsOnSlowServerRatio));

                failPoint.Dispose();

                (allCount, eventsOnSlowServerCount) = ExecuteFindOperations(collection, failPointServer.ServerId);

                var singleServerOperationsPortion = allCount / 2;
                var singleServerOperationsRange = (int)Math.Ceiling(singleServerOperationsPortion * operationsCountTolerance);

                eventsOnSlowServerCount.Should().BeInRange(singleServerOperationsPortion - singleServerOperationsRange, singleServerOperationsPortion + singleServerOperationsRange);
            }

            (int allCount, int slowServerCount) ExecuteFindOperations(IMongoCollection<BsonDocument> collection, ServerId serverId)
            {
                eventCapturer.Clear();

                ThreadingUtilities.ExecuteOnNewThreads(threadsCount, __ =>
                {
                    for (int i = 0; i < commandsPerThreadCount; i++)
                    {
                        _ = collection.Find(new BsonDocument()).FirstOrDefault();
                    }
                });

                var events = eventCapturer.Events
                    .Where(e => e is CommandStartedEvent)
                    .Cast<CommandStartedEvent>()
                    .ToArray();

                var eventsOnSlowServerCountActual = events.Where(e => e.ConnectionId.ServerId == serverId).Count();

                return (events.Length, eventsOnSlowServerCountActual);
            }
        }

        private EventCapturer CreateEventCapturer() =>
            new EventCapturer()
                .Capture<CommandStartedEvent>(e => !__commandsToNotCapture.Contains(e.CommandName));

        private void CreateCollection()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(_databaseName).WithWriteConcern(WriteConcern.WMajority);

            var collection = database.GetCollection<BsonDocument>(_collectionName);
            collection.InsertOne(new BsonDocument());
        }

        private DisposableMongoClient CreateDisposableClient(EventCapturer eventCapturer, string applicationName)
        {
            // Increase localThresholdMS and wait until all nodes are discovered to avoid false positives.
            var client = DriverTestConfiguration.CreateDisposableClient((MongoClientSettings settings) =>
                {
                    settings.Servers = settings.Servers.Take(2).ToArray();
                    settings.ApplicationName = applicationName;
                    settings.ClusterConfigurator = c => c.Subscribe(eventCapturer);
                    settings.LocalThreshold = TimeSpan.FromMilliseconds(1000);
                },
                true);
            var timeOut = TimeSpan.FromSeconds(60);
            bool AllServersConnected() => client.Cluster.Description.Servers.All(s => s.State == ServerState.Connected);
            SpinWait.SpinUntil(AllServersConnected, timeOut).Should().BeTrue();
            return client;
        }

        private void DropCollection()
        {
            var client = DriverTestConfiguration.Client;
            var database = client.GetDatabase(_databaseName).WithWriteConcern(WriteConcern.WMajority);
            database.DropCollection(_collectionName);
        }

        private void RequireMultipleShardRouters()
        {
            var connectionString = CoreTestConfiguration.ConnectionStringWithMultipleShardRouters.ToString();
            var numberOfShardRouters = MongoClientSettings.FromUrl(new MongoUrl(connectionString)).Servers.Count();
            if (numberOfShardRouters < 2)
            {
                throw new SkipException("Two or more shard routers are required.");
            }
        }
    }
}
