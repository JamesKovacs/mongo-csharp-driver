/* Copyright 2010-present MongoDB Inc.
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
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.TestHelpers.Logging;
using Xunit;
using Xunit.Abstractions;

namespace MongoDB.Driver.Tests
{
    public class LoggingTests : LoggableTestClass
    {
        public LoggingTests(ITestOutputHelper output) : base(output, includeAllCategories: true)
        {
        }

        [Fact]
        public void MongoClient_should_log()
        {
            using (var client = DriverTestConfiguration.CreateDisposableClient(LoggerFactory))
            {
                client.ListDatabases(new ListDatabasesOptions());
            }

            var logs = Logs;

            AssertLogs(new[]
            {
                (LogLevel.Information, "ICluster", "Opening"),
                (LogLevel.Information, "ICluster", "Description changed"),
                (LogLevel.Information, "IServer", "Opening"),
                (LogLevel.Information, "IConnectionPool", "Opening"),
                (LogLevel.Information, "IConnectionPool", "Opened"),
                (LogLevel.Debug,       "IServerMonitor", "Initializing"),
                (LogLevel.Debug,       "RoundTripTimeMonitor", "Monitoring started"),
                (LogLevel.Debug,       "IServerMonitor", "Initialized"),
                (LogLevel.Information, "IServer", "Opened"),
                (LogLevel.Information, "ICluster", "Opened"),
                (LogLevel.Information, "IConnectionPool", "Ready"),
                (LogLevel.Information, "IServerMonitor", "Heartbeat started"),
                (LogLevel.Information, "IConnectionPool", "Checking out connection"),
                (LogLevel.Information, "IConnectionPool", "Connection created"),
                (LogLevel.Information, "IConnection", "Opening"),
                (LogLevel.Information, "IConnection", "Sending"),
                (LogLevel.Information, "IConnection", "Sent"),
                (LogLevel.Information, "IConnection", "Receiving"),
                (LogLevel.Information, "IConnection", "Received"),
                (LogLevel.Information, "IConnectionPool", "Connection added"),
                (LogLevel.Information, "IConnectionPool", "Checked out connection"),
                (LogLevel.Information, "IConnectionPool", "Checking connection in"),
                (LogLevel.Information, "ICluster", "Closing"),
                (LogLevel.Information, "ICluster", "Removing server"),
                (LogLevel.Information, "IServer", "Closing"),
                (LogLevel.Debug,       "IServerMonitor", "Disposing"),
                (LogLevel.Debug,       "RoundTripTimeMonitor", "Disposing"),
                (LogLevel.Debug,       "RoundTripTimeMonitor", "Disposed"),
                (LogLevel.Debug,       "IServerMonitor", "Disposed"),
                (LogLevel.Information, "IConnection", "Closing"),
                (LogLevel.Information, "IConnection", "Closed"),
                (LogLevel.Information, "IConnectionPool", "Closing"),
                (LogLevel.Information, "IConnectionPool", "Closed"),
                (LogLevel.Information, "IServer", "Closed"),
                (LogLevel.Information, "ICluster", "Removed server"),
                (LogLevel.Debug,       "ICluster", "Disposing"),
                (LogLevel.Information, "ICluster", "Description changed"),
                (LogLevel.Debug,       "ICluster", "Disposed"),
                (LogLevel.Information, "ICluster", "Closed")
            },
            logs);
        }

        [Fact]
        public void MongoClient_should_not_throw_when_factory_is_null()
        {
            using (var client = DriverTestConfiguration.CreateDisposableClient(loggerFactory: null))
            {
                client.ListDatabases(new ListDatabasesOptions());
            }

            Logs.Any().Should().BeFalse();
        }

        private void AssertLogs((LogLevel logLevel, string categorySubString, string messageSubString)[] expectedLogs, LogEntry[] actualLogs)
        {
            var actualLogIndex = 0;
            foreach (var (logLevel, categorySubString, messageSubString) in expectedLogs)
            {
                actualLogIndex = Array.FindIndex(actualLogs, actualLogIndex, Match);

                if (actualLogIndex < 0)
                {
                    throw new Exception($"Log entry '{logLevel}_{categorySubString}_{messageSubString}' not found");
                }

                bool Match(LogEntry logEntry) =>
                    logEntry.LogLevel == logLevel &&
                    logEntry.Category?.Contains(categorySubString) == true &&
                    logEntry.FormattedMessage?.Contains(messageSubString) == true;
            }
        }
    }
}
