﻿/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;

namespace MongoDB.Driver.Core.Logging
{
    internal static partial class StructuredLogsTemplates
    {
        private static string[] __cmapCommonParams = new[]
            {
                ClusterId,
                ServerHost,
                ServerPort,
                Message,
            };

        private static string CmapCommonParams(params string[] @params) => Concat(__cmapCommonParams, @params);

        private static void AddCmapTemplates()
        {
            AddTemplate<ConnectionPoolAddingConnectionEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection adding"));

            AddTemplate<ConnectionPoolCheckingInConnectionEvent>(
                 LogLevel.Debug,
                 ConnectionCommonParams(),
                 e => GetParams(e.ConnectionId, "Connection checking in"));

            AddTemplate<ConnectionPoolCheckedInConnectionEvent>(
                LogLevel.Debug,
                ConnectionCommonParams(),
                e => GetParams(e.ConnectionId, "Connection checked in"));

            AddTemplate<ConnectionPoolCheckingOutConnectionEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection checkout started"));

            AddTemplate<ConnectionPoolCheckedOutConnectionEvent>(
                LogLevel.Debug,
                ConnectionCommonParams(),
                e => GetParams(e.ConnectionId, "Connection checked out"));

            AddTemplate<ConnectionPoolCheckingOutConnectionFailedEvent>(
                LogLevel.Debug,
                CmapCommonParams(Reason),
                e => GetParams(e.ServerId, "Connection checkout failed", e.Reason));

            AddTemplate<ConnectionPoolRemovingConnectionEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection removing"));

            AddTemplate<ConnectionPoolRemovedConnectionEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection removed"));

            AddTemplate<ConnectionPoolOpeningEvent, ConnectionSettings>(
                LogLevel.Debug,
                CmapCommonParams(MaxIdleTimeMS, WaitQueueTimeoutMS, MinPoolSize, MaxPoolSize, MaxConnecting),
                (e, s) => GetParams(
                    e.ServerId,
                    "Connection pool opening",
                    s.MaxIdleTime.TotalMilliseconds,
                    e.ConnectionPoolSettings.WaitQueueTimeout.TotalMilliseconds,
                    e.ConnectionPoolSettings.MinConnections,
                    e.ConnectionPoolSettings.MaxConnections,
                    e.ConnectionPoolSettings.MaxConnecting));

            AddTemplate<ConnectionPoolOpenedEvent, ConnectionSettings>(
                LogLevel.Debug,
                CmapCommonParams(MaxIdleTimeMS, WaitQueueTimeoutMS, MinPoolSize, MaxPoolSize, MaxConnecting),
                (e, s) => GetParams(
                    e.ServerId,
                    "Connection pool created",
                    s.MaxIdleTime.TotalMilliseconds,
                    e.ConnectionPoolSettings.WaitQueueTimeout.TotalMilliseconds,
                    e.ConnectionPoolSettings.MinConnections,
                    e.ConnectionPoolSettings.MaxConnections,
                    e.ConnectionPoolSettings.MaxConnecting));

            AddTemplate<ConnectionPoolReadyEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection pool ready"));

            AddTemplate<ConnectionPoolClearingEvent>(
                LogLevel.Debug,
                CmapCommonParams(ServiceId),
                e => GetParams(e.ServerId, "Connection pool clearing", e.ServiceId));

            AddTemplate<ConnectionPoolClearedEvent>(
                LogLevel.Debug,
                CmapCommonParams(ServiceId),
                e => GetParams(e.ServerId, "Connection pool cleared", e.ServiceId));

            AddTemplate<ConnectionPoolClosingEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection pool closing"));

            AddTemplate<ConnectionPoolClosedEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Connection pool closed"));
        }
    }
}
