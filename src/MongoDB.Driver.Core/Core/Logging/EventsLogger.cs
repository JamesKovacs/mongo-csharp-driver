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

using System;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Events;

namespace MongoDB.Driver.Core.Logging
{
    internal sealed class EventsLogger<T> where T : LogCategories.EventCategory
    {
        private readonly EventsPublisher _eventsPublisher;
        private readonly ILogger<T> _logger;

        public EventsLogger(IEventSubscriber eventSubscriber, ILogger<T> logger)
        {
            _logger = logger;
            _eventsPublisher = eventSubscriber != null ? new EventsPublisher(eventSubscriber) : null;
        }

        public ILogger<T> Logger => _logger;

        public bool IsEventTracked<TEvent>() where TEvent : struct, IEvent =>
            Logger?.IsEnabled(GetEventVerbosity<TEvent>()) == true ||
            _eventsPublisher?.IsEventTracked<TEvent>() == true;

        private LogLevel GetEventVerbosity<TEvent>() where TEvent : struct, IEvent =>
            StructuredLogsTemplates.GetTemplate(new TEvent().Type).LogLevel;

        public void LogAndPublish<TEvent>(TEvent @event) where TEvent : struct, IEvent
            => LogAndPublish(null, @event);

        public void LogAndPublish<TEvent>(Exception exception, TEvent @event) where TEvent : struct, IEvent
        {
            var template = StructuredLogsTemplates.GetTemplate(@event.Type);

            if (_logger?.IsEnabled(template.LogLevel) == true)
            {
                var @params = template.GetParams(@event);

                Log(template.LogLevel, template.Template, exception, @params);
            }

            _eventsPublisher?.Publish(@event);
        }

        public void LogAndPublish<TEvent, TArg>(TEvent @event, TArg arg) where TEvent : struct, IEvent
        {
            var template = StructuredLogsTemplates.GetTemplate(@event.Type);

            if (_logger?.IsEnabled(template.LogLevel) == true)
            {
                var @params = template.GetParams(@event, arg);
                Log(template.LogLevel, template.Template, exception: null, @params);
            }

            _eventsPublisher?.Publish(@event);
        }

        private void Log(LogLevel logLevel, string template, Exception exception, object[] @params)
        {
            switch (logLevel)
            {
                case LogLevel.Trace: _logger.LogTrace(exception, template, @params); break;
                case LogLevel.Debug: _logger.LogDebug(exception, template, @params); break;
                case LogLevel.Information: _logger.LogInformation(exception, template, @params); break;
                case LogLevel.Warning: _logger.LogWarning(exception, template, @params); break;
                case LogLevel.Error: _logger.LogError(exception, template, @params); break;
                case LogLevel.Critical: _logger.LogCritical(exception, template, @params); break;
                default: throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, "Unsupported log level.");
            }
        }

        //public void Log<TEvent>(TEvent @event, Action<TEvent> paramsExtractor) where TEvent : struct, IEvent
        //{
        //    if (Logger?.IsEnabled(__eventsVerbosity[(int)@event.Type]) == true)
        //    {
        //        var format = GetTemplate(@event);
        //        var @params = GetParams(@event);
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public object[] GetParams2(CommandStartedEvent @event) => null;

        //#region Command

        //public void LogAndPublish(CommandStartedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ConnectionId.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ConnectionId.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            @event.RequestId,
        //            @event.OperationId,
        //            host,
        //            port,
        //            @event.ConnectionId.ServerValue,
        //            @event.ServiceId,
        //            "Command started",
        //            @event.CommandName,
        //            @event.DatabaseNamespace.DatabaseName,
        //            @event.Command?.ToString());
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(CommandSucceededEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ConnectionId.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ConnectionId.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            @event.RequestId,
        //            @event.OperationId,
        //            host,
        //            port,
        //            @event.ConnectionId.ServerValue,
        //            @event.ServiceId,
        //            "Command succeeded",
        //            @event.CommandName,
        //            @event.Duration.TotalMilliseconds,
        //            @event.Reply?.ToString());
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(CommandFailedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ConnectionId.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            @event.Failure,
        //            GetTemplate(@event),
        //            @event.ConnectionId.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            @event.RequestId,
        //            @event.OperationId,
        //            host,
        //            port,
        //            @event.ConnectionId.ServerValue,
        //            @event.ServiceId,
        //            "Command failed",
        //            @event.CommandName,
        //            @event.Duration.TotalMilliseconds,
        //            @event.Failure.ToString());
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //#endregion

        //#region Connection

        //public void LogAndPublish(ConnectionFailedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            @event.Exception,
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection failed",
        //            @event.Exception.ToString());
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionClosingEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection closing",
        //            "Unknown");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionClosedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        Logger.LogDebug(GetTemplate(@event), GetParams(@event.ConnectionId, "Unknown"));
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionOpeningEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection opening");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionOpenedEvent @event)
        //{
        //    //LogDebug(@event.ConnectionId, me);
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection ready");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionOpeningFailedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection opening failed");
        //    }

        //    Logger?.LogDebug(@event.Exception, Id_Message_ServerId, Id, "Opening failed", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionReceivingMessageEvent @event)
        //{
        //    if (_isTraceEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogTrace(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Receiving");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionReceivedMessageEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message_ServerId, Id, "Received", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionReceivingMessageFailedEvent @event)
        //{
        //    Logger?.LogDebug(@event.Exception, Id_Message_ServerId, Id, "Receiving failed", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionSendingMessagesEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message_ServerId, Id, "Sending", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionSentMessagesEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message_ServerId, Id, "Sent", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionSendingMessagesFailedEvent @event)
        //{
        //    Logger?.LogDebug(@event.Exception, Id_Message_ServerId, Id, "Sending failed", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //#endregion

        //#region CMAP

        //public void LogAndPublish(ConnectionPoolCheckingOutConnectionEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection checkout started");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolCheckedOutConnectionEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection checked out",
        //            @event.ConnectionId.LocalValue);
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolCheckingOutConnectionFailedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection checkout failed",
        //            @event.Reason.ToString());
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolCheckingInConnectionEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection checking in",
        //            @event.ConnectionId.LocalValue);
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolCheckedInConnectionEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection checked in",
        //            @event.ConnectionId.LocalValue);
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolAddingConnectionEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection adding");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolAddedConnectionEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),

        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection added");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolOpeningEvent @event, ConnectionSettings connectionSettings)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        var poolSettings = @event.ConnectionPoolSettings;

        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection pool creating",
        //            connectionSettings?.MaxIdleTime,
        //            poolSettings.WaitQueueTimeout.TotalMilliseconds,
        //            poolSettings.MinConnections,
        //            poolSettings.MaxConnections,
        //            poolSettings.MaxConnecting);
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolOpenedEvent @event, ConnectionSettings connectionSettings)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        var poolSettings = @event.ConnectionPoolSettings;

        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection pool created",
        //            connectionSettings?.MaxIdleTime,
        //            poolSettings.MinConnections,
        //            poolSettings.MaxConnections,
        //            poolSettings.MaxConnecting);
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolReadyEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection pool ready");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolClosingEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message, Id, "Closing");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolClosedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            host,
        //            port,
        //            "Connection pool closed");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolClearingEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message, Id, "Clearing");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolClearedEvent @event)
        //{
        //    var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //    Logger?.LogDebug(
        //        GetTemplate(@event),
        //        @event.ServerId.ClusterId.Value,
        //        host,
        //        port,
        //        "Connection pool cleared",
        //        @event.ServiceId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolRemovingConnectionEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message, Id, "Removing");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionPoolRemovedConnectionEvent @event)
        //{
        //    Logger?.LogDebug(Id_Message, Id, "Removed");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ConnectionCreatedEvent @event)
        //{
        //    if (_isDebugEnabled)
        //    {
        //        var (host, port) = @event.ServerId.EndPoint.GetHostAndPort();
        //        Logger?.LogDebug(
        //            GetTemplate(@event),
        //            @event.ServerId.ClusterId.Value,
        //            @event.ConnectionId.LocalValue,
        //            host,
        //            port,
        //            "Connection created");
        //    }

        //    _eventsPublisher?.Publish(@event);
        //}

        //#endregion

        //#region Cluster

        //public void LogAndPublish(ClusterDescriptionChangedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_Description,
        //        Id,
        //        "Description changed",
        //        @event.NewDescription);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterSelectingServerEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_OperationId,
        //        Id,
        //        "Selecting server",
        //        @event.OperationId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterSelectedServerEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_OperationId,
        //        Id,
        //        "Selected server",
        //        @event.OperationId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterSelectingServerFailedEvent @event)
        //{
        //    Logger?.LogInformation(@event.Exception,
        //        Id_Message_OperationId,
        //        Id,
        //        "Selecting server failed",
        //        @event.OperationId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterClosingEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Closing");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterClosedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Closed");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterOpeningEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Opening");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterOpenedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Opened");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterAddingServerEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Adding");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterAddedServerEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_ServerId, Id, "Added server", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterRemovingServerEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_ServerId, Id, "Removing server", @event.ServerId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ClusterRemovedServerEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_ServerId_Reason_Duration,
        //        Id,
        //        "Removed server",
        //        @event.ServerId,
        //        @event.Reason,
        //        @event.Duration);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(SdamInformationEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_Information, Id, "SdamInformation", @event.Message);

        //    try
        //    {
        //        _eventsPublisher?.Publish(@event);
        //    }
        //    catch (Exception publishException)
        //    {
        //        Logger?.LogDebug(publishException, "Failed publishing SdamInformationEvent event");
        //    }
        //}

        //public void LogAndPublish(Exception ex, SdamInformationEvent @event)
        //{
        //    Logger?.LogInformation(ex, Id_Message_Information, Id, "SdamInformation", @event.Message);

        //    try
        //    {
        //        _eventsPublisher?.Publish(@event);
        //    }
        //    catch (Exception publishException)
        //    {
        //        Logger?.LogDebug(publishException, "Failed publishing SdamInformationEvent event");
        //    }
        //}

        //#endregion

        //#region SDAM

        //public void LogAndPublish(ServerHeartbeatStartedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_ConnectionId, Id, "Heartbeat started", @event.ConnectionId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ServerHeartbeatSucceededEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_ConnectionId, Id, "Heartbeat succeeded", @event.ConnectionId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ServerHeartbeatFailedEvent @event)
        //{
        //    Logger?.LogInformation(@event.Exception, Id_Message_ConnectionId, Id, "Heartbeat failed", @event.ConnectionId);

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(SdamInformationEvent @event, Exception ex)
        //{
        //    Logger?.LogInformation(ex, Id_Message_Information, Id, "SdamInformation", @event.Message);

        //    try
        //    {
        //        _eventsPublisher?.Publish(@event);
        //    }
        //    catch (Exception publishException)
        //    {
        //        // Ignore any exceptions thrown by the handler (note: event handlers aren't supposed to throw exceptions)
        //        // Backward compatibility
        //        Logger?.LogWarning(publishException, "Failed publishing event {Event}", @event);
        //    }
        //}

        //public void LogAndPublish(ServerOpeningEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Opening");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ServerOpenedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Opened");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ServerClosingEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Closing");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ServerClosedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message, Id, "Closed");

        //    _eventsPublisher?.Publish(@event);
        //}

        //public void LogAndPublish(ServerDescriptionChangedEvent @event)
        //{
        //    Logger?.LogInformation(Id_Message_Description, Id, "Description changed", @event.NewDescription);

        //    _eventsPublisher?.Publish(@event);
        //}

        //#endregion
    }
}
