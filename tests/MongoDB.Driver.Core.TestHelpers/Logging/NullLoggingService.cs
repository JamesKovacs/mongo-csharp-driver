using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MongoDB.Driver.Core.TestHelpers.Logging
{
    public sealed class NullLoggingService : ILoggingService
    {
        public ILoggerFactory LoggerFactory { get; } = NullLoggerFactory.Instance;
        public LogEntry[] Logs { get; } = new LogEntry[0];

        public static ILoggingService Instance = new NullLoggingService();

        private NullLoggingService() { }
    }
}
