using System;
using MongoDB.Driver.Core.Events;

namespace MongoDB.Driver.TestConsoleApplication
{
    public class CustomEventSubscriber : IEventSubscriber
    {
        private readonly IEventSubscriber _subscriber;

        public CustomEventSubscriber()
        {
            _subscriber = new ReflectionEventSubscriber(this);
        }

        public bool TryGetEventHandler<TEvent>(out Action<TEvent> handler)
        {
            return _subscriber.TryGetEventHandler(out handler);
        }

        #region CMAP events
        public void Handle(ConnectionPoolAddedConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} connection added to pool at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ConnectionPoolAddingConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} connection adding to pool at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolCheckedInConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} checked in connection to pool at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ConnectionPoolCheckedOutConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} checked out connection from pool at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ConnectionPoolCheckingInConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} checking in connection to pool at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolCheckingOutConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} checking out connection from pool at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolCheckingOutConnectionFailedEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} failed to check out connection for operation {e.OperationId} at {e.Timestamp}: {e.Reason}");
        }
        public void Handle(ConnectionPoolClearedEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} pool cleared at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolClearingEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} pool clearing at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolClosedEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} pool closed at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolClosingEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} pool closing at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolOpenedEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} pool opened at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolOpeningEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} pool opening at {e.Timestamp}.");
        }
        public void Handle(ConnectionPoolRemovedConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} connection removed from pool at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ConnectionPoolRemovingConnectionEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId} connection removing from pool at {e.Timestamp}.");
        }
        #endregion CMAP events

        #region Command events
        public void Handle(CommandStartedEvent e)
        {
            Log(e.GetType().Name, $"{e.ConnectionId}/{e.OperationId} started {e.CommandName} on {e.DatabaseNamespace} at {e.Timestamp}.");
        }
        public void Handle(CommandSucceededEvent e)
        {
            Log(e.GetType().Name, $"{e.ConnectionId}/{e.OperationId} {e.CommandName} succeeded at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(CommandFailedEvent e)
        {
            Log(e.GetType().Name, $"{e.ConnectionId}/{e.OperationId} {e.CommandName} failed at {e.Timestamp} took {e.Duration} ex: {e.Failure}.");
        }
        #endregion Command events

        #region SDAM events
        public void Handle(ClusterAddedServerEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} added server {e.ServerId} at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ClusterAddingServerEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} adding server {e.EndPoint} at {e.Timestamp}.");
        }
        public void Handle(ClusterClosedEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} closed at {e.Timestamp} took {e.Duration}");
        }
        public void Handle(ClusterClosingEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} closing at {e.Timestamp}");
        }
        public void Handle(ClusterDescriptionChangedEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} topology changed at {e.Timestamp} from {e.OldDescription} to {e.NewDescription}.");
        }
        public void Handle(ClusterOpenedEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} opened at {e.Timestamp} and took {e.Duration}.");
        }
        public void Handle(ClusterOpeningEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} opening at {e.Timestamp}.");
        }
        public void Handle(ClusterRemovedServerEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} removed server {e.ServerId} at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ClusterRemovingServerEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} removing server {e.ServerId} at {e.Timestamp}.");
        }
        public void Handle(SdamInformationEvent e)
        {
            Log(e.GetType().Name, $"{e.Message} received at {e.Timestamp}.");
        }
        public void Handle(ServerClosedEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} closed server {e.ServerId} at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ServerClosingEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} closing server {e.ServerId} at {e.Timestamp}.");
        }
        public void Handle(ServerDescriptionChangedEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} server description changed at {e.Timestamp} from {e.OldDescription} to {e.NewDescription}.");
        }
        public void Handle(ServerHeartbeatFailedEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId}/{e.ConnectionId} failed heartbeat at {e.Timestamp}.");
        }
        public void Handle(ServerHeartbeatStartedEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId}/{e.ConnectionId} started heartbeat at {e.Timestamp}.");
        }
        public void Handle(ServerHeartbeatSucceededEvent e)
        {
            Log(e.GetType().Name, $"{e.ServerId}/{e.ConnectionId} succeeded heartbeat at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ServerOpenedEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} opened server {e.ServerId} at {e.Timestamp} took {e.Duration}.");
        }
        public void Handle(ServerOpeningEvent e)
        {
            Log(e.GetType().Name, $"{e.ClusterId} opening server {e.ServerId} at {e.Timestamp}.");
        }

        public void Handle(DiagnosticEvent e)
        {
            Log(e.GetType().Name, $"##{e.Message}.");
        }
        #endregion SDAM events

        private void Log(string eventName, string eventDetails)
        {
            Console.WriteLine($"{DateTime.UtcNow:O}\t{eventName}\t{eventDetails}");
        }
    }
}
