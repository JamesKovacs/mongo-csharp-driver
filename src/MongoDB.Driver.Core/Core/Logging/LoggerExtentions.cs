using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Logging
{
    internal static class LoggerExtentions
    {
        public static LoggerDecorator Decorate(this ILogger logger, string id) =>
            new LoggerDecorator(logger, "Id", id);

        public static LoggerDecorator Decorate(this ILogger logger, ServerId serverId) =>
            Decorate(logger, LoggerIdFormatter.FormatId(serverId));

        public static EventsLogger ToEventsLogger(this ILogger logger, IEventSubscriber eventSubscriber, string id) =>
            new EventsLogger(eventSubscriber, logger, id);

        public static EventsLogger ToEventsLogger(this ILogger logger, IEventSubscriber eventSubscriber, ConnectionId connectionId) =>
            ToEventsLogger(logger, eventSubscriber, LoggerIdFormatter.FormatId(connectionId));

        public static EventsLogger ToEventsLogger(this ILogger logger, IEventSubscriber eventSubscriber, ServerId serverId) =>
            ToEventsLogger(logger, eventSubscriber, LoggerIdFormatter.FormatId(serverId));
    }
}
