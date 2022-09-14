using Microsoft.Extensions.Logging;

namespace MongoDB.Driver.Core.TestHelpers.Logging
{
    public interface ILoggingService
    {
        public ILoggerFactory LoggerFactory { get; }
        public LogEntry[] Logs { get; }
    }

}
