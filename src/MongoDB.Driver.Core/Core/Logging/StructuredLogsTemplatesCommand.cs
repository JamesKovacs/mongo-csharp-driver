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

using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Events;

namespace MongoDB.Driver.Core.Logging
{
    internal static partial class StructuredLogsTemplates
    {
        private static string[] __commandCommonParams = new[]
            {
                ClusterId,
                DriverConnectionId,
                ServerHost,
                ServerPort,
                ServerConnectionId,
                ServiceId,
                RequestId,
                OperationId,
                Message,
                CommandName
            };

        private static string CommandCommonParams(params string[] @params) => Concat(__commandCommonParams, @params);

        private static void AddCommandTemplates()
        {
            AddTemplate<CommandStartedEvent>(
                LogLevel.Debug,
                CommandCommonParams(DatabaseName, Command),
                e => GetParams(
                    e.ConnectionId,
                    e.ConnectionId.ServerValue,
                    e.ServiceId,
                    e.RequestId,
                    e.OperationId,
                    "Command started",
                    e.CommandName,
                    e.DatabaseNamespace.DatabaseName,
                    e.Command?.ToString()));

            AddTemplate<CommandSucceededEvent>(
                LogLevel.Debug,
                CommandCommonParams(DurationMS, Reply),
                e => GetParams(
                    e.ConnectionId,
                    e.ConnectionId.ServerValue,
                    e.ServiceId,
                    e.RequestId,
                    e.OperationId,
                    "Command succeeded",
                    e.CommandName,
                    e.Duration.TotalMilliseconds,
                    e.Reply?.ToString()));

            AddTemplate<CommandFailedEvent>(
                LogLevel.Debug,
                CommandCommonParams(DurationMS, Failure),
                e => GetParams(
                    e.ConnectionId,
                    e.ConnectionId.ServerValue,
                    e.ServiceId,
                    e.RequestId,
                    e.OperationId,
                    "Command failed",
                    e.CommandName,
                    e.Duration.TotalMilliseconds,
                    e.Failure?.ToString()));
        }
    }
}
