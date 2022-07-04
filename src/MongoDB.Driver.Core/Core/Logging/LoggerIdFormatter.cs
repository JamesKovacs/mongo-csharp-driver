using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Logging
{
    internal static class LoggerIdFormatter
    {
        public static string FormatId(ConnectionId connectionId) => $"{connectionId.LocalValue}";
        public static string FormatId(ServerId serverId) => $"{serverId.ClusterId.Value}_{EndPointHelper.ToString(serverId.EndPoint)}";
        public static string FormatId(ClusterId clusterId) => clusterId.ToString();
    }
}
