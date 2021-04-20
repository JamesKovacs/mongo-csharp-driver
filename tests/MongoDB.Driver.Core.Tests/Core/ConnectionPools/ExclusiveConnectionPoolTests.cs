/* Copyright 2013-present MongoDB Inc.
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson.TestHelpers;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Helpers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using Moq;
using Xunit;

namespace MongoDB.Driver.Core.ConnectionPools
{
    public class ExclusiveConnectionPoolTests
    {
        private Mock<IConnectionFactory> _mockConnectionFactory;
        private Mock<IConnectionExceptionHandler> _mockConnectionExceptionHandler;
        private DnsEndPoint _endPoint;
        private EventCapturer _capturedEvents;
        private ServerId _serverId;
        private ConnectionPoolSettings _settings;
        private ExclusiveConnectionPool _subject;

        public ExclusiveConnectionPoolTests()
        {
            _mockConnectionFactory = new Mock<IConnectionFactory> { DefaultValue = DefaultValue.Mock };
            _mockConnectionExceptionHandler = new Mock<IConnectionExceptionHandler>();
            _endPoint = new DnsEndPoint("localhost", 27017);
            _capturedEvents = new EventCapturer();
            _serverId = new ServerId(new ClusterId(), _endPoint);
            _mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();
                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());
                    return connectionMock.Object;
                });
            _settings = new ConnectionPoolSettings(
                maintenanceInterval: Timeout.InfiniteTimeSpan,
                maxConnections: 4,
                minConnections: 2,
                waitQueueSize: 1,
                waitQueueTimeout: TimeSpan.FromSeconds(2));

            _subject = CreateSubject();
        }

        [Fact]
        public void Constructor_should_throw_when_serverId_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(null, _endPoint, _settings, _mockConnectionFactory.Object, _capturedEvents, _mockConnectionExceptionHandler.Object);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_endPoint_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, null, _settings, _mockConnectionFactory.Object, _capturedEvents, _mockConnectionExceptionHandler.Object);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_settings_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, null, _mockConnectionFactory.Object, _capturedEvents, _mockConnectionExceptionHandler.Object);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_connectionFactory_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, _settings, null, _capturedEvents, _mockConnectionExceptionHandler.Object);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_eventSubscriber_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, _settings, _mockConnectionFactory.Object, null, _mockConnectionExceptionHandler.Object);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_exceptionHandler_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, _settings, _mockConnectionFactory.Object, _capturedEvents, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquireConnection_should_throw_an_InvalidOperationException_if_not_initialized(
            [Values(false, true)]
            bool async)
        {
            Action act;
            if (async)
            {
                act = () => _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                act = () => _subject.AcquireConnection(CancellationToken.None);
            }

            act.ShouldThrow<InvalidOperationException>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
            var connectionPoolCheckingOutConnectionFailedEvent = _capturedEvents.Next();
            var e = connectionPoolCheckingOutConnectionFailedEvent.Should().BeOfType<ConnectionPoolCheckingOutConnectionFailedEvent>().Subject;
            e.Reason.Should().Be(ConnectionCheckOutFailedReason.ConnectionError);
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquireConnection_should_throw_an_ObjectDisposedException_after_disposing(
            [Values(false, true)]
            bool async)
        {
            _capturedEvents.Clear();
            _subject.Dispose();

            Action act;
            if (async)
            {
                act = () => _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                act = () => _subject.AcquireConnection(CancellationToken.None);
            }

            act.ShouldThrow<ObjectDisposedException>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolClosingEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolClosedEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
            var connectionPoolCheckingOutConnectionFailedEvent = _capturedEvents.Next();
            var e = connectionPoolCheckingOutConnectionFailedEvent.Should().BeOfType<ConnectionPoolCheckingOutConnectionFailedEvent>().Subject;
            e.Reason.Should().Be(ConnectionCheckOutFailedReason.PoolClosed);
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquireConnection_should_return_a_connection(
            [Values(false, true)]
            bool async)
        {
            InitializeAndWait();
            _capturedEvents.Clear();

            IConnectionHandle connection;
            if (async)
            {
                connection = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                connection = _subject.AcquireConnection(CancellationToken.None);
            }

            connection.Should().NotBeNull();
            _subject.AvailableCount.Should().Be(_settings.MaxConnections - 1);
            _subject.CreatedCount.Should().Be(_settings.MinConnections);
            _subject.DormantCount.Should().Be(_settings.MinConnections - 1);
            _subject.UsedCount.Should().Be(1);

            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedOutConnectionEvent>();
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquireConnection_should_increase_count_up_to_the_max_number_of_connections(
            [Values(false, true)]
            bool async)
        {
            InitializeAndWait();
            _capturedEvents.Clear();

            var connections = new List<IConnection>();

            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                IConnection connection;
                if (async)
                {
                    connection = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    connection = _subject.AcquireConnection(CancellationToken.None);
                }
                connections.Add(connection);
            }

            _subject.AvailableCount.Should().Be(0);
            _subject.CreatedCount.Should().Be(_settings.MaxConnections);
            _subject.DormantCount.Should().Be(0);
            _subject.UsedCount.Should().Be(_settings.MaxConnections);

            for (int i = 0; i < _settings.MinConnections; i++)
            {
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedOutConnectionEvent>();
            }
            for (int i = _settings.MinConnections; i < _settings.MaxConnections; i++)
            {
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolAddingConnectionEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionCreatedEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolAddedConnectionEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedOutConnectionEvent>();
            }
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquiredConnection_should_return_connections_to_the_pool_when_disposed(
            [Values(false, true)]
            bool async)
        {
            InitializeAndWait();

            IConnectionHandle connection;
            if (async)
            {
                connection = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                connection = _subject.AcquireConnection(CancellationToken.None);
            }

            _capturedEvents.Clear();

            _subject.DormantCount.Should().Be(_settings.MinConnections - 1);
            connection.Dispose();
            _subject.DormantCount.Should().Be(_settings.MinConnections);

            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedInConnectionEvent>();
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquiredConnection_should_not_return_connections_to_the_pool_when_disposed_and_expired(
            [Values(false, true)]
            bool async)
        {
            var createdConnections = new List<MockConnection>();
            _mockConnectionFactory.Setup(f => f.CreateConnection(_serverId, _endPoint))
                .Returns(() =>
                {
                    var conn = new MockConnection(_serverId);
                    createdConnections.Add(conn);
                    return conn;
                });

            InitializeAndWait();

            IConnectionHandle connection;
            if (async)
            {
                connection = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                connection = _subject.AcquireConnection(CancellationToken.None);
            }

            _capturedEvents.Clear();

            _subject.DormantCount.Should().Be(_settings.MinConnections - 1);

            createdConnections.ForEach(c => c.IsExpired = true);

            connection.Dispose();
            _subject.DormantCount.Should().Be(_settings.MinConnections - 1);

            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolRemovingConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolRemovedConnectionEvent>();
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquireConnection_should_throw_a_TimeoutException_when_all_connections_are_checked_out(
            [Values(false, true)]
            bool async)
        {
            InitializeAndWait();
            var connections = new List<IConnection>();
            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                IConnection connection;
                if (async)
                {
                    connection = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
                }
                else
                {
                    connection = _subject.AcquireConnection(CancellationToken.None);
                }
                connections.Add(connection);
            }
            _capturedEvents.Clear();

            Action act;
            if (async)
            {
                act = () => _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                act = () => _subject.AcquireConnection(CancellationToken.None);
            }

            act.ShouldThrow<TimeoutException>();

            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingOutConnectionEvent>();
            var connectionPoolCheckingOutConnectionFailedEvent = _capturedEvents.Next();
            var e = connectionPoolCheckingOutConnectionFailedEvent.Should().BeOfType<ConnectionPoolCheckingOutConnectionFailedEvent>().Subject;
            e.Reason.Should().Be(ConnectionCheckOutFailedReason.Timeout);
            _capturedEvents.Any().Should().BeFalse();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquiredConnection_should_not_throw_exceptions_when_disposed_after_the_pool_was_disposed(
            [Values(false, true)]
            bool async)
        {
            InitializeAndWait();
            IConnectionHandle connection1;
            IConnectionHandle connection2;
            if (async)
            {
                connection1 = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
                connection2 = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                connection1 = _subject.AcquireConnection(CancellationToken.None);
                connection2 = _subject.AcquireConnection(CancellationToken.None);
            }
            _capturedEvents.Clear();

            connection1.Dispose();
            _subject.Dispose();

            Action act = () => connection2.Dispose();
            act.ShouldNotThrow();

            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolClosingEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolRemovingConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolRemovedConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolClosedEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckingInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolCheckedInConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolRemovingConnectionEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolRemovedConnectionEvent>();
            _capturedEvents.Any().Should().BeFalse();
        }

        [Fact]
        public void AcquireConnection_should_iterate_over_all_dormant_connections()
        {
            const int connectionsCount = 10;

            var settings = _settings.With(
                minConnections: 0,
                maxConnections: connectionsCount,
                waitQueueTimeout: TimeSpan.FromMilliseconds(1),
                maintenanceInterval: TimeSpan.FromMilliseconds(10000000));

            var connectionsCreated = new HashSet<ConnectionId>();
            var connectionsExpired = new HashSet<ConnectionId>();
            var connectionsDisposed = new HashSet<ConnectionId>();

            var syncRoot = new object();

            var mockConnectionFactory = new Mock<IConnectionFactory> { DefaultValue = DefaultValue.Mock };
            mockConnectionFactory
                .Setup(f => f.CreateConnection(_serverId, _endPoint))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();

                    connectionMock
                        .Setup(c => c.ConnectionId)
                        .Returns(new ConnectionId(_serverId));

                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                        .Setup(c => c.IsExpired)
                        .Returns(() =>
                        {
                            lock (syncRoot)
                            {
                                return connectionsExpired.Contains(connectionMock.Object.ConnectionId);
                            }
                        });

                    connectionMock
                        .Setup(c => c.Dispose())
                        .Callback(() => connectionsDisposed.Add(connectionMock.Object.ConnectionId));

                    connectionsCreated.Add(connectionMock.Object.ConnectionId);

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();
            subject.SetReady();

            // acquire all connections and return them
            var allConnections = Enumerable.Range(0, connectionsCount)
                .Select(i => subject.AcquireConnection(default))
                .ToArray();

            var connectionNotToExpire = allConnections[allConnections.Length / 2].ConnectionId;

            foreach (var connection in allConnections)
            {
                connection.Dispose();
            }

            subject.DormantCount.Should().Be(connectionsCount);

            // expire all of the connections expect one
            _capturedEvents.Clear();
            var random = new Random();
            var connectionsToExpire = new List<ConnectionId>();
            lock (syncRoot)
            {
                foreach (var connectionId in connectionsCreated)
                {
                    connectionsExpired.Add(connectionId);
                }

                connectionsExpired.Remove(connectionNotToExpire);
            }

            // acquire connection again, no new connections should be created, some expired connections should be removed
            AcquireConnectionGeneric(subject, true).Should().NotBeNull();

            // ensure no new connections where created
            subject.DormantCount.Should().Be(connectionsCount - connectionsDisposed.Count - 1);
        }

        [Fact]
        public void Clear_should_throw_an_InvalidOperationException_if_not_initialized()
        {
            Action act = () => _subject.Clear();

            act.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Clear_should_throw_an_ObjectDisposedException_after_disposing()
        {
            _subject.Dispose();

            Action act = () => _subject.Clear();

            act.ShouldThrow<ObjectDisposedException>();
        }

        [Theory]
        [ParameterAttributeData]
        public void Clear_should_cause_existing_connections_to_be_expired(
            [Values(false, true)]
            bool async)
        {
            _subject.Initialize();
            _subject.SetReady();

            IConnectionHandle connection;
            if (async)
            {
                connection = _subject.AcquireConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            else
            {
                connection = _subject.AcquireConnection(CancellationToken.None);
            }

            connection.IsExpired.Should().BeFalse();
            _subject.Clear();
            connection.IsExpired.Should().BeTrue();
        }

        [Fact]
        public void Initialize_should_throw_an_ObjectDisposedException_after_disposing()
        {
            _subject.Dispose();

            Action act = () => _subject.Initialize();

            act.ShouldThrow<ObjectDisposedException>();
        }

        [Fact]
        public void Initialize_should_scale_up_the_number_of_connections_to_min_size()
        {
            _subject.CreatedCount.Should().Be(0);
            _subject.DormantCount.Should().Be(0);
            InitializeAndWait();

            _capturedEvents.Next().Should().BeOfType<ConnectionPoolOpeningEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolOpenedEvent>();
            _capturedEvents.Next().Should().BeOfType<ConnectionPoolReadyEvent>();

            for (int i = 0; i < _settings.MinConnections; i++)
            {
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolAddingConnectionEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionCreatedEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolAddedConnectionEvent>();
            }
        }

        [Fact]
        public async Task MaintainSizeAsync_should_call_connection_dispose_when_connection_authentication_fail()
        {
            var authenticationException = new MongoAuthenticationException(new ConnectionId(_serverId), "test message");
            var authenticationFailedConnection = new Mock<IConnection>();
            authenticationFailedConnection
                .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())) // an authentication exception is thrown from _connectionInitializer.InitializeConnection
                                                                        // that in turn is called from OpenAsync
                .Throws(authenticationException);

            using (var subject = CreateSubject())
            {
                var tokenSource = new CancellationTokenSource();
                _mockConnectionFactory
                    .Setup(f => f.CreateConnection(_serverId, _endPoint))
                    .Returns(() =>
                    {
                        tokenSource.Cancel();
                        return authenticationFailedConnection.Object;
                    });

                await subject.MaintainSizeAsync(tokenSource.Token);

                authenticationFailedConnection.Verify(conn => conn.Dispose(), Times.Once);
                _mockConnectionExceptionHandler.Verify(c => c.HandleException(authenticationException), Times.Once);
            }
        }

        [Fact]
        public void MaintainSizeAsync_should_not_try_new_attempt_after_failing_without_delay()
        {
            var settings = _settings.With(maintenanceInterval: TimeSpan.FromSeconds(10));

            using (var subject = CreateSubject(settings))
            {
                var tokenSource = new CancellationTokenSource();
                _mockConnectionFactory
                    .SetupSequence(f => f.CreateConnection(_serverId, _endPoint))
                    .Throws<Exception>()    // failed attempt
                    .Returns(() =>          // successful attempt which should be delayed
                    {
                        // break the loop. With this line the MaintainSizeAsync will contain only 2 iterations
                        tokenSource.Cancel();
                        return new MockConnection(_serverId);
                    });

                var testResult = Task.WaitAny(
                    subject.MaintainSizeAsync(tokenSource.Token), // if this task is completed first, it will mean that there was no delay (10 sec) 
                    Task.Delay(TimeSpan.FromSeconds(1)));         // time to be sure that delay is happening,
                                                                  // if the method is running more than 1 second, then delay is happening
                testResult.Should().Be(1);
            }
        }

        [Fact]
        public void PrunePoolAsync_should_remove_all_expired_connections()
        {
            const int connectionsCount = 10;

            var settings = _settings.With(
                minConnections: connectionsCount,
                maxConnections: connectionsCount,
                maintenanceInterval: TimeSpan.FromMilliseconds(10));

            var connectionsCreated = new HashSet<ConnectionId>();
            var connectionsExpired = new HashSet<ConnectionId>();
            var connectionsDisposed = new HashSet<ConnectionId>();

            var syncRoot = new object();

            var mockConnectionFactory = new Mock<IConnectionFactory> { DefaultValue = DefaultValue.Mock };
            mockConnectionFactory
                .Setup(f => f.CreateConnection(_serverId, _endPoint))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();

                    connectionMock
                        .Setup(c => c.ConnectionId)
                        .Returns(new ConnectionId(_serverId));

                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                        .Setup(c => c.IsExpired)
                        .Returns(() =>
                        {
                            lock (syncRoot)
                            {
                                return connectionsExpired.Contains(connectionMock.Object.ConnectionId);
                            }
                        });

                    connectionMock
                        .Setup(c => c.Dispose())
                        .Callback(() => connectionsDisposed.Add(connectionMock.Object.ConnectionId));

                    connectionsCreated.Add(connectionMock.Object.ConnectionId);

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();
            subject.SetReady();

            _capturedEvents.WaitForOrThrowIfTimeout(events => events.Where(e => e is ConnectionCreatedEvent).Count() >= connectionsCount, TimeSpan.FromSeconds(10));
            subject.DormantCount.Should().Be(connectionsCount);

            // expired some of the connections
            _capturedEvents.Clear();
            var random = new Random();
            var connectionsToExpire = new List<ConnectionId>();
            lock (syncRoot)
            {
                // add at least one connection
                connectionsExpired.Add(connectionsCreated.First());

                foreach (var connectionId in connectionsCreated.Skip(1))
                {
                    if (random.NextDouble() > 0.5)
                    {
                        connectionsExpired.Add(connectionId);
                    }
                }
            }

            // ensure removed events are received in subsequent order, meaning all expired connections where removed in same pass
            _capturedEvents.WaitForOrThrowIfTimeout(events => events.Count() >= connectionsExpired.Count * 2, TimeSpan.FromSeconds(10));

            _capturedEvents.Events
                .Take(connectionsCount * 2)
                .Where(e => e is ConnectionPoolRemovingConnectionEvent)
                .Cast<ConnectionPoolRemovingConnectionEvent>()
                .Select(e => e.ConnectionId.LocalValue)
                .ShouldBeEquivalentTo(connectionsExpired.Select(c => c.LocalValue));

            _capturedEvents.Events
                .Take(connectionsCount * 2)
                .Where(e => e is ConnectionPoolRemovedConnectionEvent)
                .Cast<ConnectionPoolRemovedConnectionEvent>()
                .Select(e => e.ConnectionId.LocalValue)
                .ShouldAllBeEquivalentTo(connectionsExpired.Select(c => c.LocalValue));
        }

        [Theory]
        [ParameterAttributeData]
        public void AquireConnection_non_sufficient_reused_connections_should_timeout(
            [Values(true, false)]
            bool isAsync,
            [Values(1, 10, null)]
            int? maxConnectingOptional)
        {
            int maxConnecting = maxConnectingOptional ?? MongoCoreDefaults.ConnectionPool.MaxConnecting;
            int initalAcquiredCount = maxConnecting;
            int maxAcquiringCount = maxConnecting * 2;
            const int queueTimeoutMS = 50;

            var settings = _settings.WithInternal(
                waitQueueSize: maxAcquiringCount + initalAcquiredCount + maxConnecting,
                maxConnections: maxAcquiringCount + initalAcquiredCount + maxConnecting,
                maxConnecting: maxConnecting,
                waitQueueTimeout: TimeSpan.FromMilliseconds(queueTimeoutMS),
                minConnections: 0);

            var allAcquiringCountEvent = new CountdownEvent(maxAcquiringCount + initalAcquiredCount);
            var blockEstablishmentEvent = new ManualResetEventSlim(true);
            var establishingCount = new CountdownEvent(maxConnecting + initalAcquiredCount);

            var mockConnectionFactory = new Mock<IConnectionFactory>();
            mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();

                    connectionMock
                        .Setup(c => c.ConnectionId)
                        .Returns(new ConnectionId(_serverId));

                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                        .Setup(c => c.Open(It.IsAny<CancellationToken>()))
                        .Callback(() =>
                        {
                            if (establishingCount.CurrentCount > 0)
                            {
                                establishingCount.Signal();
                            }

                            blockEstablishmentEvent.Wait();
                        });

                    connectionMock
                        .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                        .Returns(() =>
                        {
                            if (establishingCount.CurrentCount > 0)
                            {
                                establishingCount.Signal();
                            }

                            blockEstablishmentEvent.Wait();
                            return Task.FromResult(0);
                        });

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();
            subject.SetReady();

            subject.PendingCount.Should().Be(0);
            var connectionsAcquired = Enumerable.Range(0, initalAcquiredCount)
                .Select(i => AcquireConnectionGeneric(subject, isAsync))
                .ToArray();

            // block further establishments
            blockEstablishmentEvent.Reset();

            var allConnections = new List<IConnection>();
            var actualTimeouts = 0;
            var expectedTimeouts = maxAcquiringCount - maxConnecting;

            ThreadingUtilities.ExecuteOnNewThreads(maxAcquiringCount + maxConnecting + 1, threadIndex =>
            {
                if (threadIndex < maxConnecting)
                {
                    // maximize maxConnecting
                    allAcquiringCountEvent.Signal();
                    AcquireConnectionGeneric(subject, isAsync);
                }
                else if (threadIndex < maxConnecting + maxAcquiringCount)
                {
                    // wait until all maxConnecting maximized
                    establishingCount.Wait();
                    subject.PendingCount.Should().Be(maxConnecting);

                    allAcquiringCountEvent.Signal();

                    try
                    {
                        AcquireConnectionGeneric(subject, isAsync);
                    }
                    catch (TimeoutException)
                    {
                        Interlocked.Increment(ref actualTimeouts);
                    }

                    // speedup the test
                    if (expectedTimeouts == actualTimeouts)
                    {
                        blockEstablishmentEvent.Set();
                    }
                }
                else
                {
                    // wait until all trying to acquire
                    allAcquiringCountEvent.Wait();

                    // return connections
                    foreach (var connection in connectionsAcquired)
                    {
                        connection.Dispose();
                    }
                }
            });

            expectedTimeouts.Should().Be(expectedTimeouts);
            subject.PendingCount.Should().Be(0);
        }

        [Theory]
        [ParameterAttributeData]
        public void WaitQueue_should_throw_when_full(
            [Values(true, false)] bool isAsync,
            [Values(1, 10)] int waitQueueSize)
        {
            var maxConnections = waitQueueSize + 1;
            var settings = _settings.WithInternal(
                minConnections: 0,
                maxConnections: maxConnections,
                maxConnecting: maxConnections,
                waitQueueSize: waitQueueSize,
                waitQueueTimeout: TimeSpan.FromSeconds(10));

            var blockEstablishmentEvent = new ManualResetEventSlim(false);
            var allAcquiringCountdownEvent = new CountdownEvent(waitQueueSize);

            var mockConnectionFactory = new Mock<IConnectionFactory>();
            mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();

                    connectionMock
                        .Setup(c => c.ConnectionId)
                        .Returns(new ConnectionId(_serverId));

                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                       .Setup(c => c.Open(It.IsAny<CancellationToken>()))
                       .Callback(() =>
                       {
                           allAcquiringCountdownEvent.Signal();
                           blockEstablishmentEvent.Wait();
                       });

                    connectionMock
                        .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                        .Returns(() =>
                        {
                            allAcquiringCountdownEvent.Signal();
                            blockEstablishmentEvent.Wait();
                            return Task.FromResult(1);
                        });

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();
            subject.SetReady();

            subject.WaitQueueFreeSlots().Should().Be(waitQueueSize);

            MongoWaitQueueFullException exception = null;

            ThreadingUtilities.ExecuteOnNewThreads(maxConnections, threadIndex =>
                {
                    if (threadIndex < waitQueueSize)
                    {
                        using var connection = AcquireConnectionGeneric(subject, isAsync);
                    }
                    else
                    {
                        allAcquiringCountdownEvent.Wait();

                        try
                        {
                            using var connection = AcquireConnectionGeneric(subject, isAsync);
                        }
                        catch (MongoWaitQueueFullException ex)
                        {
                            exception = ex;
                        }
                        finally
                        {
                            blockEstablishmentEvent.Set();
                        }
                    }
                });

            exception.Should().NotBeNull();
            subject.WaitQueueFreeSlots().Should().Be(waitQueueSize);
        }

        [Theory]
        [ParameterAttributeData]
        public void WaitQueue_should_be_cleared_on_pool_clear(
            [Values(true, false)] bool isAsync,
            [Values(1, 2, 5)] int blockedInQueueCount)
        {
            const int maxConnecting = 2;
            var threadsCount = maxConnecting + blockedInQueueCount;
            var waitQueueSize = threadsCount;
            var settings = _settings.WithInternal(
                minConnections: 0,
                maxConnections: maxConnecting,
                maxConnecting: maxConnecting,
                waitQueueSize: waitQueueSize,
                waitQueueTimeout: TimeSpan.FromMinutes(10));

            var allEstablishing = new CountdownEvent(maxConnecting);
            var blockEstablishmentEvent = new ManualResetEventSlim(false);

            var mockConnectionFactory = new Mock<IConnectionFactory>();
            mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();

                    connectionMock
                        .Setup(c => c.ConnectionId)
                        .Returns(new ConnectionId(_serverId));

                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                       .Setup(c => c.Open(It.IsAny<CancellationToken>()))
                       .Callback(() =>
                       {
                           blockEstablishmentEvent.Wait();
                       });

                    connectionMock
                        .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                        .Returns(() =>
                        {
                            blockEstablishmentEvent.Wait();
                            return Task.FromResult(1);
                        });

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();
            subject.SetReady();

            var poolQueue = subject._poolQueue();

            var exceptions = ThreadingUtilities.ExecuteOnNewThreadsCollectExceptions(threadsCount + 1, threadIndex =>
            {
                if (threadIndex < threadsCount)
                {
                    using var connection = AcquireConnectionGeneric(subject, isAsync);
                }
                else
                {
                    SpinWait.SpinUntil(() => subject.WaitQueueFreeSlots() == 0);
                    subject.Clear();

                    SpinWait.SpinUntil(() => subject.WaitQueueFreeSlots() >= blockedInQueueCount);
                    blockEstablishmentEvent.Set();
                };
            });

            exceptions.Length.ShouldBeEquivalentTo(blockedInQueueCount);
            foreach (var e in exceptions)
            {
                e.Should().BeOfType<MongoPoolPausedException>();
            }

            subject.WaitQueueFreeSlots().Should().Be(waitQueueSize);
        }

        [Theory]
        [ParameterAttributeData]
        public void WaitQueue_should_release_slot_after_connection_checkout(
            [Values(true, false)] bool isAsync,
            [Values(1, 10)] int waitQueueSize)
        {
            var settings = _settings.With(
                waitQueueSize: waitQueueSize,
                maxConnections: waitQueueSize,
                minConnections: 0);

            var mockConnectionFactory = new Mock<IConnectionFactory>();
            mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();

                    connectionMock
                        .Setup(c => c.ConnectionId)
                        .Returns(new ConnectionId(_serverId));

                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();
            subject.SetReady();

            subject.WaitQueueFreeSlots().Should().Be(waitQueueSize);

            ThreadingUtilities.ExecuteOnNewThreads(waitQueueSize, threadIndex =>
            {
                using var connection = AcquireConnectionGeneric(subject, isAsync);
            });

            subject.WaitQueueFreeSlots().Should().Be(waitQueueSize);
        }

        // private methods
        private static IConnection AcquireConnectionGeneric(ExclusiveConnectionPool subject, bool async)
        {
            if (async)
            {
                return subject.AcquireConnectionAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            }
            else
            {
                return subject.AcquireConnection(CancellationToken.None);
            }
        }

        private ExclusiveConnectionPool CreateSubject(ConnectionPoolSettings connectionPoolSettings = null, IConnectionFactory connectionFactory = null)
        {
            return new ExclusiveConnectionPool(
                _serverId,
                _endPoint,
                connectionPoolSettings ?? _settings,
                connectionFactory ?? _mockConnectionFactory.Object,
                _capturedEvents,
                _mockConnectionExceptionHandler.Object);
        }

        private void InitializeAndWait()
        {
            _subject.Initialize();
            _subject.SetReady();

            SpinWait.SpinUntil(
                () => _subject.CreatedCount == _settings.MinConnections &&
                    _subject.AvailableCount == _settings.MaxConnections &&
                    _subject.DormantCount == _settings.MinConnections &&
                    _subject.UsedCount == 0,
                TimeSpan.FromSeconds(5))
                .Should().BeTrue();

            _subject.AvailableCount.Should().Be(_settings.MaxConnections);
            _subject.CreatedCount.Should().Be(_settings.MinConnections);
            _subject.DormantCount.Should().Be(_settings.MinConnections);
            _subject.UsedCount.Should().Be(0);
        }
    }

    internal static class ExclusiveConnectionPoolReflector
    {
        public static Task MaintainSizeAsync(this ExclusiveConnectionPool obj, CancellationToken cancellationToken)
        {
            return (Task)Reflector.Invoke(obj, nameof(MaintainSizeAsync), cancellationToken);
        }

        public static SemaphoreSlimSignalable _poolQueue(this ExclusiveConnectionPool obj)
        {
            return (SemaphoreSlimSignalable)Reflector.GetFieldValue(obj, nameof(_poolQueue));
        }

        public static int WaitQueueFreeSlots(this ExclusiveConnectionPool obj)
        {
            return (int)Reflector.GetPropertyValue(obj, nameof(WaitQueueFreeSlots));
        }
    }
}
