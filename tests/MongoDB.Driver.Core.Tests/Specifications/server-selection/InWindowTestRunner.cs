/* Copyright 2021-present MongoDB Inc.
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
using System.Net;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.TestHelpers.JsonDrivenTests;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Servers;
using Moq;
using Xunit;

namespace MongoDB.Driver.Specifications.server_selection
{
    public sealed class InWindowTestRunner
    {
        private static class Schema
        {
            public static readonly string description = nameof(description);
            public static readonly string topology_description = nameof(topology_description);
            public static readonly string mocked_topology_state = nameof(mocked_topology_state);
            public static readonly string iterations = nameof(iterations);
            public static readonly string outcome = nameof(outcome);

            public static class MockedTopologyState
            {
                public static readonly string address = nameof(address);
                public static readonly string operation_count = nameof(operation_count);
            }

            public static class Outcome
            {
                public static readonly string tolerance = nameof(tolerance);
                public static readonly string expected_frequencies = nameof(expected_frequencies);
            }

            public static readonly string[] RootFields = new[]
            {
                "_path",
                description,
                topology_description,
                mocked_topology_state,
                iterations,
                outcome
            };
        }

        private sealed class Outcome
        {
            public double tolerance { get; set; }
            public Dictionary<string, double> expected_frequencies { get; set; }
        }

        private sealed class OperationsCount
        {
            public string address { get; set; }
            public int operation_count { get; set; }
        }

        [Theory]
        [ClassData(typeof(TestCaseFactory))]
        public void RunTestDefinition(JsonDrivenTestCase testCase)
        {
            var testDefinition = testCase.Test;

            JsonDrivenHelper.EnsureAllFieldsAreValid(testDefinition, Schema.RootFields);

            var topologyDescription = (BsonDocument)testDefinition[Schema.topology_description];
            var clusterDescription = ServerSelectionTestHelper.BuildClusterDescription(topologyDescription);
            var mockedTopolyState = ReadMockedTopologyState(testDefinition[Schema.mocked_topology_state]);
            var iterations = testDefinition[Schema.iterations].AsInt32;
            var outcome = BsonSerializer.Deserialize<Outcome>((BsonDocument)testDefinition[Schema.outcome]);

            using var cluster = CreateAndSetupCluster(clusterDescription, mockedTopolyState);
            var readPreferenceSelector = new ReadPreferenceServerSelector(ReadPreference.Nearest);

            var selectionHistogram = outcome.expected_frequencies.Keys
                .ToDictionary(s => clusterDescription.Servers.Single(d => d.EndPoint.ToString().EndsWith(s)).ServerId, s => 0);
            var selectionFrequenciesExpected = outcome.expected_frequencies.
                ToDictionary(s => clusterDescription.Servers.Single(d => d.EndPoint.ToString().EndsWith(s.Key)).ServerId, s => s.Value);

            for (int i = 0; i < iterations; i++)
            {
                var selectedServer = cluster.SelectServer(readPreferenceSelector, default);

                selectionHistogram[selectedServer.ServerId]++;
            }

            foreach (var pair in selectionHistogram)
            {
                var expectedFrequency = selectionFrequenciesExpected[pair.Key];
                var actualFrequency = pair.Value / (double)iterations;

                actualFrequency.Should().BeInRange(expectedFrequency - outcome.tolerance, expectedFrequency + outcome.tolerance);
            }
        }

        private MultiServerCluster CreateAndSetupCluster(ClusterDescription clusterDescription, OperationsCount[] operationsCounts)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var clusterSettings = new ClusterSettings(
                connectionMode: clusterDescription.ConnectionMode,
                connectionModeSwitch: clusterDescription.ConnectionModeSwitch,
                serverSelectionTimeout: TimeSpan.FromSeconds(30),
                endPoints: new Optional<IEnumerable<EndPoint>>(clusterDescription.Servers.Select(s => s.EndPoint)));
#pragma warning restore CS0618 // Type or member is obsolete

            var mockServerFactory = new Mock<IClusterableServerFactory>();
            mockServerFactory
                .Setup(s => s.CreateServer(It.IsAny<ClusterId>(), It.IsAny<IClusterClock>(), It.IsAny<EndPoint>()))
                .Returns<ClusterId, IClusterClock, EndPoint>((_, _, endpoint) =>
                {
                    var serverDescription = clusterDescription.Servers
                        .Single(s => s.EndPoint == endpoint).
                        With(state: ServerState.Connected);

                    var operationsCount = operationsCounts.Single(o => endpoint.ToString().EndsWith(o.address));

                    var server = new Mock<IClusterableServer>();
                    server.Setup(s => s.ServerId).Returns(serverDescription.ServerId);
                    server.Setup(s => s.Description).Returns(serverDescription);
                    server.Setup(s => s.EndPoint).Returns(endpoint);
                    server.Setup(s => s.OutstandingOperationsCount).Returns(operationsCount.operation_count);

                    return server.Object;
                });

            var result = new MultiServerCluster(clusterSettings, mockServerFactory.Object, new EventCapturer());
            result.Initialize();
            return result;
        }

        private OperationsCount[] ReadMockedTopologyState(BsonValue mockTopologyState) =>
            ((BsonArray)mockTopologyState)
                .Select(d => BsonSerializer.Deserialize<OperationsCount>((BsonDocument)d))
                .ToArray();

        // nested types
        private class TestCaseFactory : JsonDrivenTestCaseFactory
        {
            protected override string[] PathPrefixes => new[]
            {
                "MongoDB.Driver.Core.Tests.Specifications.server_selection.tests.in_window.",
            };

            protected override IEnumerable<JsonDrivenTestCase> CreateTestCases(BsonDocument document)
            {
                var name = GetTestCaseName(document, document, 0);
                yield return new JsonDrivenTestCase(name, document, document);
            }
        }
    }
}
