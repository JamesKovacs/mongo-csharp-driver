using System;
using Microsoft.Extensions.Logging;

namespace MongoDB.Driver.Core.Logging
{
    internal class LoggerDecorator
    {
        public ILogger Logger { get; }
        public string DecorationName { get; }
        public string DecorationValue { get; }

        public LoggerDecorator(ILogger logger, string decorationName, string decorationValue)
        {
            Logger = logger;
            DecorationName = decorationName;
            DecorationValue = decorationValue;
        }

        public void LogDebug(string message)
        {
            Logger?.LogDebug($"{{{DecorationName}}} {message}", DecorationValue);
        }

        public void LogDebug(Exception exception, string message)
        {
            Logger?.LogDebug(exception, $"{{{DecorationName}}} {message}", DecorationValue);
        }

        public void LogDebug(string message, object arg)
        {
            Logger?.LogDebug($"{{{DecorationName}}} {message}", DecorationValue, arg);
        }

        public void LogInformation(string message)
        {
            Logger?.LogInformation($"{{{DecorationName}}} {message}", DecorationValue);
        }

        public void LogWarning(string message)
        {
            Logger?.LogWarning($"{{{DecorationName}}} {message}", DecorationValue);
        }
    }
}
