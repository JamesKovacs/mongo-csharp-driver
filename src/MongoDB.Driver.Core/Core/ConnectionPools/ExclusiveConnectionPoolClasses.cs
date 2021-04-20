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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol.Messages;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.ConnectionPools
{
    internal sealed partial class ExclusiveConnectionPool : IConnectionPool
    {
        // nested classes
        private enum State
        {
            Initial,
            Paused,
            Ready,
            Disposed
        }

        private sealed class PoolState
        {
            private static readonly bool[,] __transitions;
            private readonly InterlockedEnumInt32<State> _state;

            static PoolState()
            {
                __transitions = new bool[4, 4];
                __transitions[(int)State.Initial, (int)State.Paused] = true;
                __transitions[(int)State.Paused, (int)State.Paused] = true;
                __transitions[(int)State.Ready, (int)State.Ready] = true;
                __transitions[(int)State.Ready, (int)State.Paused] = true;
                __transitions[(int)State.Paused, (int)State.Ready] = true;

                __transitions[(int)State.Initial, (int)State.Disposed] = true;
                __transitions[(int)State.Paused, (int)State.Disposed] = true;
                __transitions[(int)State.Ready, (int)State.Disposed] = true;
                __transitions[(int)State.Disposed, (int)State.Disposed] = true;
            }

            public PoolState()
            {
                _state = new InterlockedEnumInt32<State>(State.Initial);
            }

            public State State => _state.Value;
            public bool IsDisposed => _state.Value == State.Disposed;
            public bool IsNotDisposed => _state.Value != State.Disposed;

            public bool TransitionState(State newState)
            {
                State currentState;

                do
                {
                    currentState = _state.Value;

                    if (!__transitions[(int)currentState, (int)newState])
                    {
                        ThrowIfDisposed(currentState);

                        throw new InvalidOperationException($"Invalid transition {currentState} to {newState}");
                    }
                }
                while (currentState != newState && _state.CompareExchange(currentState, newState) != currentState);

                return currentState != newState;
            }

            public void ThrowIfDisposed() => ThrowIfDisposed(_state.Value);

            public void ThrowIfDisposedOrNotReady()
            {
                ThrowIfDisposed();

                var state = _state.Value;
                if (state != State.Ready)
                {
                    throw new InvalidOperationException($"ConnectionPool must be ready, but is in {state} state.");
                }
            }

            public void ThrowIfNotInitialized()
            {
                if (_state.Value == State.Initial)
                {
                    throw new InvalidOperationException("ConnectionPool must be initialized.");
                }
            }

            public override string ToString() => State.ToString();

            // private methods
            private void ThrowIfDisposed(State state)
            {
                if (state == State.Disposed)
                {
                    throw new ObjectDisposedException(typeof(ExclusiveConnectionPool).Name);
                }
            }
        }

        private sealed class MaintenanceState : IDisposable
        {
            private readonly object _syncRoot = new object();

            private CancellationTokenSource _cancellationTokenSource = null;
            private Task _maintenanceTask;

            public void Cancel()
            {
                lock (_syncRoot)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = null;
                    _maintenanceTask = null;
                }
            }

            public void Start(Func<CancellationToken, Task> maintenanceTaskCreator)
            {
                lock (_syncRoot)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();

                    _maintenanceTask = maintenanceTaskCreator(_cancellationTokenSource.Token);
                    _maintenanceTask.ConfigureAwait(false);
                }
            }

            public void Dispose()
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
        }

        private sealed class AcquireConnectionHelper : IDisposable
        {
            // private fields
            private readonly ExclusiveConnectionPool _pool;
            private readonly TimeSpan _timeout;

            private bool _enteredWaitQueue;
            private SemaphoreSlimSignalable.SemaphoreWaitResult _poolQueueWaitResult;

            // constructors
            public AcquireConnectionHelper(ExclusiveConnectionPool pool)
            {
                _pool = pool;
                _timeout = pool._settings.WaitQueueTimeout;
            }

            public IConnectionHandle AcquireConnection(CancellationToken cancellationToken)
            {
                try
                {
                    StartCheckingOut();

                    var stopwatch = Stopwatch.StartNew();
                    _poolQueueWaitResult = _pool._poolQueue.WaitSignaled(_timeout, cancellationToken);

                    if (_poolQueueWaitResult == SemaphoreSlimSignalable.SemaphoreWaitResult.Entered)
                    {
                        PooledConnection pooledConnection = null;
                        var timeout = EnsureTimeout(stopwatch);

                        using (var connectionCreator = new ConnectionCreator(_pool, timeout))
                        {
                            pooledConnection = connectionCreator.CreateOpenedOrReuse(cancellationToken);
                        }

                        return EndCheckingOut(pooledConnection, stopwatch);
                    }

                    stopwatch.Stop();
                    throw CreateException(stopwatch);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    throw;
                }
            }

            public async Task<IConnectionHandle> AcquireConnectionAsync(CancellationToken cancellationToken)
            {
                try
                {
                    StartCheckingOut();

                    var stopwatch = Stopwatch.StartNew();
                    _poolQueueWaitResult = await _pool._poolQueue.WaitSignaledAsync(_timeout, cancellationToken).ConfigureAwait(false);

                    if (_poolQueueWaitResult == SemaphoreSlimSignalable.SemaphoreWaitResult.Entered)
                    {
                        PooledConnection pooledConnection = null;
                        var timeout = EnsureTimeout(stopwatch);

                        using (var connectionCreator = new ConnectionCreator(_pool, timeout))
                        {
                            pooledConnection = await connectionCreator.CreateOpenedOrReuseAsync(cancellationToken).ConfigureAwait(false);
                        }

                        return EndCheckingOut(pooledConnection, stopwatch);
                    }

                    stopwatch.Stop();
                    throw CreateException(stopwatch);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                    throw;
                }
            }

            public void Dispose()
            {
                if (_enteredWaitQueue)
                {
                    Interlocked.Increment(ref _pool._waitQueueFreeSlots);
                }

                if (_poolQueueWaitResult == SemaphoreSlimSignalable.SemaphoreWaitResult.Entered)
                {
                    try
                    {
                        _pool._poolQueue.Release();
                    }
                    catch
                    {
                        // TODO: log this, but don't throw... it's a bug if we get here
                    }
                }
            }

            // private methods
            private void StartCheckingOut()
            {
                _pool._checkingOutConnectionEventHandler?
                    .Invoke(new ConnectionPoolCheckingOutConnectionEvent(_pool._serverId, EventContext.OperationId));

                _pool._poolState.ThrowIfDisposedOrNotReady();

                // enter the wait-queue, deprecated feature
                int freeSlots;
                do
                {
                    freeSlots = _pool._waitQueueFreeSlots;

                    if (freeSlots == 0)
                    {
                        throw MongoWaitQueueFullException.ForConnectionPool(_pool._endPoint);
                    }
                }
                while (Interlocked.CompareExchange(ref _pool._waitQueueFreeSlots, freeSlots - 1, freeSlots) != freeSlots);

                _enteredWaitQueue = true;
            }

            private IConnectionHandle EndCheckingOut(PooledConnection pooledConnection, Stopwatch stopwatch)
            {
                var reference = new ReferenceCounted<PooledConnection>(pooledConnection, _pool.ReleaseConnection);
                var connectionHandle = new AcquiredConnection(_pool, reference);

                stopwatch.Stop();
                var checkedOutConnectionEvent = new ConnectionPoolCheckedOutConnectionEvent(connectionHandle.ConnectionId, stopwatch.Elapsed, EventContext.OperationId);
                _pool._checkedOutConnectionEventHandler?.Invoke(checkedOutConnectionEvent);

                // no need to release the semaphore
                _poolQueueWaitResult = SemaphoreSlimSignalable.SemaphoreWaitResult.None;

                return connectionHandle;
            }

            private TimeSpan EnsureTimeout(Stopwatch stopwatch)
            {
                var timeSpentInWaitQueue = stopwatch.Elapsed;
                var timeout = _timeout - timeSpentInWaitQueue;

                if (timeout < TimeSpan.Zero)
                {
                    throw new TimeoutException($"Timed out waiting for a connection after {timeSpentInWaitQueue}ms.");
                }

                return timeout;
            }

            private Exception CreateException(Stopwatch stopwatch) =>
                _poolQueueWaitResult switch
                {
                    SemaphoreSlimSignalable.SemaphoreWaitResult.Signaled =>
                        MongoPoolPausedException.ForConnectionPool(_pool._endPoint),
                    SemaphoreSlimSignalable.SemaphoreWaitResult.TimedOut =>
                        new TimeoutException($"Timed out waiting for a connection after {stopwatch.ElapsedMilliseconds}ms."),
                    _ => new ArgumentOutOfRangeException(nameof(_poolQueueWaitResult))
                };

            private void HandleException(Exception ex)
            {
                var handler = _pool._checkingOutConnectionFailedEventHandler;
                if (handler != null)
                {
                    ConnectionCheckOutFailedReason reason;
                    switch (ex)
                    {
                        case ObjectDisposedException _: reason = ConnectionCheckOutFailedReason.PoolClosed; break;
                        case TimeoutException _: reason = ConnectionCheckOutFailedReason.Timeout; break;
                        default: reason = ConnectionCheckOutFailedReason.ConnectionError; break;
                    }
                    handler(new ConnectionPoolCheckingOutConnectionFailedEvent(_pool._serverId, ex, EventContext.OperationId, reason));
                }
            }
        }

        private sealed class PooledConnection : IConnection
        {
            private readonly IConnection _connection;
            private readonly ExclusiveConnectionPool _connectionPool;
            private readonly int _generation;

            public PooledConnection(ExclusiveConnectionPool connectionPool, IConnection connection)
            {
                _connectionPool = connectionPool;
                _connection = connection;
                _generation = connectionPool._generation;
            }

            public ConnectionId ConnectionId
            {
                get { return _connection.ConnectionId; }
            }

            public ConnectionDescription Description
            {
                get { return _connection.Description; }
            }

            public EndPoint EndPoint
            {
                get { return _connection.EndPoint; }
            }

            public int Generation
            {
                get { return _generation; }
            }

            public bool IsExpired
            {
                get { return _generation < _connectionPool.Generation || _connection.IsExpired; }
            }

            public ConnectionSettings Settings
            {
                get { return _connection.Settings; }
            }

            public void Dispose()
            {
                _connection.Dispose();
            }

            public void Open(CancellationToken cancellationToken)
            {
                try
                {
                    _connection.Open(cancellationToken);
                }
                catch (MongoConnectionException ex)
                {
                    // TODO temporary workaround for propagating exception generation to server
                    // Will be reconsider after SDAM spec error handling adjustments
                    ex.Generation = Generation;
                    throw;
                }
            }

            public async Task OpenAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (MongoConnectionException ex)
                {
                    // TODO temporary workaround for propagating exception generation to server
                    // Will be reconsider after SDAM spec error handling adjustments
                    ex.Generation = Generation;
                    throw;
                }
            }

            public ResponseMessage ReceiveMessage(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.ReceiveMessage(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.ReceiveMessageAsync(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public void SendMessages(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                _connection.SendMessages(messages, messageEncoderSettings, cancellationToken);
            }

            public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                return _connection.SendMessagesAsync(messages, messageEncoderSettings, cancellationToken);
            }

            public void SetReadTimeout(TimeSpan timeout)
            {
                _connection.SetReadTimeout(timeout);
            }
        }

        private sealed class AcquiredConnection : IConnectionHandle
        {
            private ExclusiveConnectionPool _connectionPool;
            private bool _disposed;
            private ReferenceCounted<PooledConnection> _reference;

            public AcquiredConnection(ExclusiveConnectionPool connectionPool, ReferenceCounted<PooledConnection> reference)
            {
                _connectionPool = connectionPool;
                _reference = reference;
            }

            public ConnectionId ConnectionId
            {
                get { return _reference.Instance.ConnectionId; }
            }

            public ConnectionDescription Description
            {
                get { return _reference.Instance.Description; }
            }

            public EndPoint EndPoint
            {
                get { return _reference.Instance.EndPoint; }
            }

            public int Generation
            {
                get { return _reference.Instance.Generation; }
            }

            public bool IsExpired
            {
                get
                {
                    return _connectionPool._poolState.IsDisposed || _reference.Instance.IsExpired;
                }
            }

            public ConnectionSettings Settings
            {
                get { return _reference.Instance.Settings; }
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _reference.DecrementReferenceCount();
                    _disposed = true;
                }
            }

            public IConnectionHandle Fork()
            {
                ThrowIfDisposed();
                _reference.IncrementReferenceCount();
                return new AcquiredConnection(_connectionPool, _reference);
            }

            public void Open(CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                _reference.Instance.Open(cancellationToken);
            }

            public Task OpenAsync(CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.OpenAsync(cancellationToken);
            }

            public Task<ResponseMessage> ReceiveMessageAsync(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.ReceiveMessageAsync(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public ResponseMessage ReceiveMessage(int responseTo, IMessageEncoderSelector encoderSelector, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.ReceiveMessage(responseTo, encoderSelector, messageEncoderSettings, cancellationToken);
            }

            public void SendMessages(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                _reference.Instance.SendMessages(messages, messageEncoderSettings, cancellationToken);
            }

            public Task SendMessagesAsync(IEnumerable<RequestMessage> messages, MessageEncoderSettings messageEncoderSettings, CancellationToken cancellationToken)
            {
                ThrowIfDisposed();
                return _reference.Instance.SendMessagesAsync(messages, messageEncoderSettings, cancellationToken);
            }

            public void SetReadTimeout(TimeSpan timeout)
            {
                ThrowIfDisposed();
                _reference.Instance.SetReadTimeout(timeout);
            }

            private void ThrowIfDisposed()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }
        }

        private sealed class ListConnectionHolder
        {
            private readonly SemaphoreSlimSignalable _semaphoreSlimSignalable;
            private readonly object _lock = new object();
            private readonly List<PooledConnection> _connections;

            private readonly Action<ConnectionPoolRemovingConnectionEvent> _removingConnectionEventHandler;
            private readonly Action<ConnectionPoolRemovedConnectionEvent> _removedConnectionEventHandler;

            public ListConnectionHolder(IEventSubscriber eventSubscriber, SemaphoreSlimSignalable semaphoreSlimSignalable)
            {
                _semaphoreSlimSignalable = semaphoreSlimSignalable;
                _connections = new List<PooledConnection>();

                eventSubscriber.TryGetEventHandler(out _removingConnectionEventHandler);
                eventSubscriber.TryGetEventHandler(out _removedConnectionEventHandler);
            }

            public int Count
            {
                get
                {
                    lock (_lock)
                    {
                        return _connections.Count;
                    }
                }
            }

            public void Clear()
            {
                lock (_lock)
                {
                    foreach (var connection in _connections)
                    {
                        RemoveConnection(connection);
                    }
                    _connections.Clear();

                    SignalOrReset();
                }
            }

            public void Prune()
            {
                lock (_lock)
                {
                    var expiredConnections = _connections.Where(c => c.IsExpired).ToArray();

                    foreach (var connection in expiredConnections)
                    {
                        RemoveConnection(connection);
                        _connections.Remove(connection);
                    }

                    SignalOrReset();
                }
            }

            public PooledConnection Acquire()
            {
                PooledConnection result = null;

                lock (_lock)
                {
                    while (_connections.Count > 0 && result == null)
                    {
                        var connection = _connections.Last();
                        _connections.RemoveAt(_connections.Count - 1);

                        if (connection.IsExpired)
                        {
                            RemoveConnection(connection);
                        }
                        else
                        {
                            result = connection;
                        }
                    }

                    SignalOrReset();
                }

                return result;
            }

            public void Return(PooledConnection connection)
            {
                lock (_lock)
                {
                    _connections.Add(connection);
                    SignalOrReset();
                }
            }

            public void RemoveConnection(PooledConnection connection)
            {
                if (_removingConnectionEventHandler != null)
                {
                    _removingConnectionEventHandler(new ConnectionPoolRemovingConnectionEvent(connection.ConnectionId, EventContext.OperationId));
                }

                var stopwatch = Stopwatch.StartNew();
                connection.Dispose();
                stopwatch.Stop();

                if (_removedConnectionEventHandler != null)
                {
                    _removedConnectionEventHandler(new ConnectionPoolRemovedConnectionEvent(connection.ConnectionId, stopwatch.Elapsed, EventContext.OperationId));
                }
            }

            private void SignalOrReset()
            {
                // Should be invoked under lock only
                if (_connections.Count == 0)
                {
                    // no connections are available, clear the signal flag
                    _semaphoreSlimSignalable.Reset();
                }
                else
                {
                    // signal that connections are available
                    _semaphoreSlimSignalable.Signal();
                }
            }
        }

        private sealed class ConnectionCreator : IDisposable
        {
            private readonly ExclusiveConnectionPool _pool;
            private readonly TimeSpan _connectingTimeout;

            private PooledConnection _connection;
            private bool _disposeConnection;

            private SemaphoreSlimSignalable.SemaphoreWaitResult _connectingWaitStatus;

            private Stopwatch _stopwatch;

            public ConnectionCreator(ExclusiveConnectionPool pool, TimeSpan connectingTimeout)
            {
                _pool = pool;
                _connectingTimeout = connectingTimeout;
                _connectingWaitStatus = SemaphoreSlimSignalable.SemaphoreWaitResult.None;
                _connection = null;
                _disposeConnection = true;
                _stopwatch = null;
            }

            public async Task<PooledConnection> CreateOpenedAsync(CancellationToken cancellationToken)
            {
                var stopwatch = Stopwatch.StartNew();
                _connectingWaitStatus = await _pool._connectingQueue.WaitAsync(_connectingTimeout, cancellationToken).ConfigureAwait(false);
                stopwatch.Stop();

                if (_connectingWaitStatus == SemaphoreSlimSignalable.SemaphoreWaitResult.TimedOut)
                {
                    throw new TimeoutException($"Timed out waiting for in connecting queue after {stopwatch.ElapsedMilliseconds}ms.");
                }

                var connection = await CreateOpenedInternalAsync(cancellationToken).ConfigureAwait(false);
                return connection;
            }

            public PooledConnection CreateOpenedOrReuse(CancellationToken cancellationToken)
            {
                var connection = _pool._connectionHolder.Acquire();
                var waitTimeout = _connectingTimeout;
                var stopwatch = Stopwatch.StartNew();

                while (connection == null)
                {
                    // Try to acquire connecting semaphore. Possible operation results:
                    // Entered: The request was successfully fulfilled, and a connection establishment can start
                    // Signaled: The request was interrupted because Connection was return to pool and can be reused
                    // Timeout: The request was timed out after WaitQueueTimeout period.
                    _connectingWaitStatus = _pool._connectingQueue.WaitSignaled(waitTimeout, cancellationToken);

                    connection = _connectingWaitStatus switch
                    {
                        SemaphoreSlimSignalable.SemaphoreWaitResult.Signaled => _pool._connectionHolder.Acquire(),
                        SemaphoreSlimSignalable.SemaphoreWaitResult.Entered => CreateOpenedInternal(cancellationToken),
                        SemaphoreSlimSignalable.SemaphoreWaitResult.TimedOut => throw new TimeoutException($"Timed out waiting in connecting queue after {stopwatch.ElapsedMilliseconds}ms."),
                        _ => throw new ArgumentOutOfRangeException(nameof(_connectingWaitStatus))
                    };

                    waitTimeout = _connectingTimeout - stopwatch.Elapsed;

                    if (connection == null && waitTimeout <= TimeSpan.Zero)
                    {
                        throw TimoutException(stopwatch);
                    }
                }

                return connection;
            }

            public async Task<PooledConnection> CreateOpenedOrReuseAsync(CancellationToken cancellationToken)
            {
                var connection = _pool._connectionHolder.Acquire();

                var waitTimeout = _connectingTimeout;
                var stopwatch = Stopwatch.StartNew();

                while (connection == null)
                {
                    // Try to acquire connecting semaphore. Possible operation results:
                    // Entered: The request was successfully fulfilled, and a connection establishment can start
                    // Signaled: The request was interrupted because Connection was return to pool and can be reused
                    // Timeout: The request was timed out after WaitQueueTimeout period.
                    _connectingWaitStatus = await _pool._connectingQueue.WaitSignaledAsync(waitTimeout, cancellationToken).ConfigureAwait(false);

                    connection = _connectingWaitStatus switch
                    {
                        SemaphoreSlimSignalable.SemaphoreWaitResult.Signaled => _pool._connectionHolder.Acquire(),
                        SemaphoreSlimSignalable.SemaphoreWaitResult.Entered => await CreateOpenedInternalAsync(cancellationToken).ConfigureAwait(false),
                        SemaphoreSlimSignalable.SemaphoreWaitResult.TimedOut => throw TimoutException(stopwatch),
                        _ => throw new ArgumentOutOfRangeException(nameof(_connectingWaitStatus))
                    };

                    waitTimeout = _connectingTimeout - stopwatch.Elapsed;

                    if (connection == null && waitTimeout <= TimeSpan.Zero)
                    {
                        throw TimoutException(stopwatch);
                    }
                }

                return connection;
            }

            // private methods
            private PooledConnection CreateOpenedInternal(CancellationToken cancellationToken)
            {
                StartCreating(cancellationToken);

                _connection.Open(cancellationToken);

                FinishCreating();

                return _connection;
            }

            private async Task<PooledConnection> CreateOpenedInternalAsync(CancellationToken cancellationToken)
            {
                StartCreating(cancellationToken);

                await _connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                FinishCreating();

                return _connection;
            }

            private void StartCreating(CancellationToken cancellationToken)
            {
                var addingConnectionEvent = new ConnectionPoolAddingConnectionEvent(_pool._serverId, EventContext.OperationId);
                _pool._addingConnectionEventHandler?.Invoke(addingConnectionEvent);

                cancellationToken.ThrowIfCancellationRequested();

                _stopwatch = Stopwatch.StartNew();
                _connection = _pool.CreateNewConnection();
            }

            private void FinishCreating()
            {
                _stopwatch.Stop();

                var connectionAddedEvent = new ConnectionPoolAddedConnectionEvent(_connection.ConnectionId, _stopwatch.Elapsed, EventContext.OperationId);
                _pool._addedConnectionEventHandler?.Invoke(connectionAddedEvent);

                // Only if reached this stage, connection should not be disposed
                _disposeConnection = false;
            }

            private Exception TimoutException(Stopwatch stopwatch) =>
                new TimeoutException($"Timed out waiting in connecting queue after {stopwatch.ElapsedMilliseconds}ms.");

            public void Dispose()
            {
                if (_connectingWaitStatus == SemaphoreSlimSignalable.SemaphoreWaitResult.Entered)
                {
                    _pool._connectingQueue.Release();
                }

                if (_disposeConnection)
                {
                    // TODO SDAM spec: topology.handle_pre_handshake_error(error) # if possible, defer error handling to SDAM
                    _connection?.Dispose();
                }
            }
        }
    }
}
