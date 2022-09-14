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
        private static string[] __sdamCommonParams = new[]
            {
                ClusterId,
                DriverConnectionId,
                ServerHost,
                ServerPort,
                Message,
            };

        private static string SdamCommonParams(params string[] @params) => Concat(__sdamCommonParams, @params);

        private static void AddSdamTemplates()
        {
            AddTemplate<ServerHeartbeatStartedEvent>(
                LogLevel.Debug,
                SdamCommonParams(),
                e => GetParams(e.ConnectionId, "Heartbeat started"));

            AddTemplate<ServerHeartbeatSucceededEvent>(
                LogLevel.Debug,
                SdamCommonParams(),
                e => GetParams(e.ConnectionId, "Heartbeat succeeded"));

            AddTemplate<ServerHeartbeatFailedEvent>(
                LogLevel.Debug,
                SdamCommonParams(),
                e => GetParams(e.ConnectionId, "Heartbeat failed"));

            AddTemplate<SdamInformationEvent>(
                LogLevel.Debug,
                Concat(new[] { Message }),
                e => new[] { e.Message });

            AddTemplate<ServerOpeningEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Server opening"));

            AddTemplate<ServerOpenedEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Server opened"));

            AddTemplate<ServerClosingEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Server closing"));

            AddTemplate<ServerClosedEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Server closed"));

            AddTemplate<ServerDescriptionChangedEvent>(
                LogLevel.Debug,
                CmapCommonParams(Description),
                e => GetParams(e.ServerId, "Description changed", e.NewDescription));
        }
    }
}
