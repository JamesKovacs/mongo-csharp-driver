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
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers.JsonDrivenTests;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Specifications.server_selection
{
    internal static class ServerSelectionTestHelper
    {
        private static class Schema
        {
            public static readonly string topology_description = nameof(topology_description);

            public static class TopologyDescription
            {
                public static readonly string servers = nameof(servers);
                public static readonly string type = nameof(type);

                public static class ClusterType
                {
                    public static readonly string ReplicaSet = nameof(ReplicaSet);
                    public static readonly string Sharded = nameof(Sharded);
                    public static readonly string Single = nameof(Single);
                    public static readonly string Unknown = nameof(Unknown);
                }

                public static class Server
                {
                    public static readonly string address = nameof(address);
                    public static readonly string avg_rtt_ms = nameof(avg_rtt_ms);
                    public static readonly string tags = nameof(tags);
                    public static readonly string type = nameof(type);
                    public static readonly string lastUpdateTime = nameof(lastUpdateTime);
                    public static readonly string lastWrite = nameof(lastWrite);
                    public static readonly string maxWireVersion = nameof(maxWireVersion);

                    public static readonly string[] AllFields = new[]
                    {
                        address,
                        avg_rtt_ms,
                        tags,
                        type,
                        lastUpdateTime,
                        lastWrite,
                        maxWireVersion
                    };
                }

                public static class ServerType
                {
                    public static readonly string RSPrimary = nameof(RSPrimary);
                    public static readonly string RSSecondary = nameof(RSSecondary);
                    public static readonly string RSArbiter = nameof(RSArbiter);
                    public static readonly string RSGhost = nameof(RSGhost);
                    public static readonly string RSOther = nameof(RSOther);
                    public static readonly string Mongos = nameof(Mongos);
                    public static readonly string Standalone = nameof(Standalone);
                }

                public static class ServerTags
                {
                    public static readonly string data_center = nameof(data_center);
                    public static readonly string rack = nameof(rack);
                    public static readonly string other_tag = nameof(other_tag);

                    public static readonly string[] AllFields = new[]
                    {
                        data_center,
                        rack,
                        other_tag,
                    };
                }
            }
        }

        public static ClusterDescription BuildClusterDescription(
            BsonDocument topologyDescription,
            TimeSpan? heartbeatInterval = null)
        {
            var clusterId = new ClusterId();

            heartbeatInterval = heartbeatInterval ?? TimeSpan.FromMilliseconds(500);
            JsonDrivenHelper.EnsureAllFieldsAreValid(topologyDescription, Schema.TopologyDescription.servers, Schema.TopologyDescription.type);

            var (clusterType, clusterConnectionMode) = GetClusterType(topologyDescription[Schema.TopologyDescription.type].ToString());
            var servers = BuildServerDescriptions((BsonArray)topologyDescription[Schema.TopologyDescription.servers], clusterId, heartbeatInterval.Value);

#pragma warning disable CS0618 // Type or member is obsolete
            return new ClusterDescription(clusterId, clusterConnectionMode, clusterType, servers);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static List<ServerDescription> BuildServerDescriptions(BsonArray serverDescriptions, ClusterId clusterId, TimeSpan heartbeatInterval) =>
            serverDescriptions.Select(x => BuildServerDescription((BsonDocument)x, clusterId, heartbeatInterval)).ToList();

        // private methods
#pragma warning disable CS0618 // Type or member is obsolete
        private static (ClusterType, ClusterConnectionMode) GetClusterType(string type)
        {
            if (type.StartsWith(Schema.TopologyDescription.ClusterType.ReplicaSet))
            {
                return (ClusterType.ReplicaSet, ClusterConnectionMode.ReplicaSet);
            }

            if (type == Schema.TopologyDescription.ClusterType.Sharded)
            {
                return (ClusterType.Sharded, ClusterConnectionMode.Sharded);
            }

            if (type == Schema.TopologyDescription.ClusterType.Single)
            {
                return (ClusterType.Standalone, ClusterConnectionMode.Standalone);
            }

            if (type == Schema.TopologyDescription.ClusterType.Unknown)
            {
                return (ClusterType.Unknown, ClusterConnectionMode.Automatic);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            throw new NotSupportedException("Unknown topology type: " + type);
        }

        private static ServerDescription BuildServerDescription(
            BsonDocument serverDescription,
            ClusterId clusterId,
            TimeSpan heartbeatInterval)
        {
            var utcNow = DateTime.UtcNow;

            JsonDrivenHelper.EnsureAllFieldsAreValid(serverDescription, Schema.TopologyDescription.Server.AllFields);

            var endPoint = EndPointHelper.Parse(serverDescription[Schema.TopologyDescription.Server.address].ToString());
            var averageRoundTripTime = TimeSpan.FromMilliseconds(serverDescription.GetValue(Schema.TopologyDescription.Server.avg_rtt_ms, 0.0).ToDouble());
            var type = GetServerType(serverDescription[Schema.TopologyDescription.Server.type].ToString());

            TagSet tagSet = null;
            if (serverDescription.TryGetValue(Schema.TopologyDescription.Server.tags, out var tagsElement))
            {
                tagSet = BuildTagSet((BsonDocument)tagsElement);
            }
            DateTime lastWriteTimestamp;
            if (serverDescription.TryGetValue(Schema.TopologyDescription.Server.lastWrite, out var lastWrite))
            {
                lastWriteTimestamp = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(lastWrite["lastWriteDate"].ToInt64());
            }
            else
            {
                lastWriteTimestamp = utcNow;
            }
            var maxWireVersion = serverDescription.GetValue(Schema.TopologyDescription.Server.maxWireVersion, 5).ToInt32();
            var wireVersionRange = new Range<int>(0, maxWireVersion);
            var serverVersion = maxWireVersion == 5 ? new SemanticVersion(3, 4, 0) : new SemanticVersion(3, 2, 0);
            DateTime lastUpdateTimestamp;
            if (serverDescription.TryGetValue(Schema.TopologyDescription.Server.lastUpdateTime, out var lastUpdateTime))
            {
                lastUpdateTimestamp = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(lastUpdateTime.ToInt64());
            }
            else
            {
                lastUpdateTimestamp = utcNow;
            }

            var serverId = new ServerId(clusterId, endPoint);
            return new ServerDescription(
                serverId,
                endPoint,
                averageRoundTripTime: averageRoundTripTime,
                type: type,
                lastUpdateTimestamp: lastUpdateTimestamp,
                lastWriteTimestamp: lastWriteTimestamp,
                heartbeatInterval: heartbeatInterval,
                wireVersionRange: wireVersionRange,
                version: serverVersion,
                tags: tagSet,
                state: ServerState.Connected);
        }

        private static TagSet BuildTagSet(BsonDocument tagSet)
        {
            JsonDrivenHelper.EnsureAllFieldsAreValid(tagSet, Schema.TopologyDescription.ServerTags.AllFields);

            return new TagSet(tagSet.Elements.Select(x => new Tag(x.Name, x.Value.ToString())));
        }

        private static ServerType GetServerType(string type)
        {
            if (type == Schema.TopologyDescription.ServerType.RSPrimary)
            {
                return ServerType.ReplicaSetPrimary;
            }
            else if (type == Schema.TopologyDescription.ServerType.RSSecondary)
            {
                return ServerType.ReplicaSetSecondary;
            }
            else if (type == Schema.TopologyDescription.ServerType.RSArbiter)
            {
                return ServerType.ReplicaSetArbiter;
            }
            else if (type == Schema.TopologyDescription.ServerType.RSGhost)
            {
                return ServerType.ReplicaSetGhost;
            }
            else if (type == Schema.TopologyDescription.ServerType.RSOther)
            {
                return ServerType.ReplicaSetOther;
            }
            else if (type == Schema.TopologyDescription.ServerType.Mongos)
            {
                return ServerType.ShardRouter;
            }
            else if (type == Schema.TopologyDescription.ServerType.Standalone)
            {
                return ServerType.Standalone;
            }
            else
            {
                return ServerType.Unknown;
            }
        }
    }
}
