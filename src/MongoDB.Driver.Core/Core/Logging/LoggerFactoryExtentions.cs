using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Logging
{
    internal static class LoggerFactoryExtentions
    {
        public static EventsLogger CreateEventsLogger<T>(this ILoggerFactory loggerFactory, IEventSubscriber eventSubscriber, string id) =>
            new EventsLogger(eventSubscriber, loggerFactory?.CreateLogger<T>(), id);

        public static EventsLogger CreateEventsLogger<T>(this ILoggerFactory loggerFactory, IEventSubscriber eventSubscriber, ConnectionId id) =>
            CreateEventsLogger<T>(loggerFactory, eventSubscriber, LoggerIdFormatter.FormatId(id));

        public static EventsLogger CreateEventsLogger<T>(this ILoggerFactory loggerFactory, IEventSubscriber eventSubscriber, ClusterId id) =>
            CreateEventsLogger<T>(loggerFactory, eventSubscriber, LoggerIdFormatter.FormatId(id));

        public static EventsLogger CreateEventsLogger<T>(this ILoggerFactory loggerFactory, IEventSubscriber eventSubscriber, ServerId id) =>
            CreateEventsLogger<T>(loggerFactory, eventSubscriber, LoggerIdFormatter.FormatId(id));
    }
}
