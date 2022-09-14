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
        private static string[] __clusterCommonParams = new[]
            {
                ClusterId,
                Message,
            };

        private static string ClusterCommonParams(params string[] @params) => Concat(__clusterCommonParams, @params);

        private static void AddClusterTemplates()
        {
            AddTemplate<ClusterDescriptionChangedEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Description changed"));

            AddTemplate<ClusterSelectingServerEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Selecting server"));

            AddTemplate<ClusterSelectedServerEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Selected server"));

            AddTemplate<ClusterSelectingServerFailedEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Selecting server failed"));

            AddTemplate<ClusterClosingEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Cluster closing"));

            AddTemplate<ClusterClosedEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Cluster closed"));

            AddTemplate<ClusterOpeningEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Cluster opening"));

            AddTemplate<ClusterOpenedEvent>(
                LogLevel.Debug,
                ClusterCommonParams(),
                e => GetParams(e.ClusterId, "Cluster opened"));

            AddTemplate<ClusterAddingServerEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ClusterId, e.EndPoint, "Adding server"));

            AddTemplate<ClusterAddedServerEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Added server"));

            AddTemplate<ClusterRemovingServerEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Removing server"));

            AddTemplate<ClusterRemovedServerEvent>(
                LogLevel.Debug,
                CmapCommonParams(),
                e => GetParams(e.ServerId, "Removed server"));
        }
    }
}
