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
using System.Collections.Concurrent;
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
using MongoDB.Driver.Core.Servers;
using Moq;
using Xunit;

namespace MongoDB.Driver.Core.ConnectionPools
{
    public class ExclusiveConnectionPoolTests
    {
        private Mock<IConnectionFactory> _mockConnectionFactory;
        private DnsEndPoint _endPoint;
        private EventCapturer _capturedEvents;
        private ServerId _serverId;
        private ConnectionPoolSettings _settings;
        private ExclusiveConnectionPool _subject;

        public ExclusiveConnectionPoolTests()
        {
            _mockConnectionFactory = new Mock<IConnectionFactory> { DefaultValue = DefaultValue.Mock };
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
            Action act = () => new ExclusiveConnectionPool(null, _endPoint, _settings, _mockConnectionFactory.Object, _capturedEvents);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_endPoint_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, null, _settings, _mockConnectionFactory.Object, _capturedEvents);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_settings_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, null, _mockConnectionFactory.Object, _capturedEvents);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_connectionFactory_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, _settings, null, _capturedEvents);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_eventSubscriber_is_null()
        {
            Action act = () => new ExclusiveConnectionPool(_serverId, _endPoint, _settings, _mockConnectionFactory.Object, null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [ParameterAttributeData]
        public void AcquireConnection_should_throw_an_InvalidOperationException_if_not_initialized(
            [Values(false, true)]
            bool async)
        {
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

            for (int i = 0; i < _settings.MinConnections; i++)
            {
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolAddingConnectionEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionCreatedEvent>();
                _capturedEvents.Next().Should().BeOfType<ConnectionPoolAddedConnectionEvent>();
            }
        }

        [Fact]
        public void MaintainSizeAsync_should_call_connection_dispose_when_connection_authentication_fail()
        {
            var authenticationFailedConnection = new Mock<IConnection>();
            authenticationFailedConnection
                .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>())) // an authentication exception is thrown from _connectionInitializer.InitializeConnection
                                                                        // that in turn is called from OpenAsync
                .Throws(new MongoAuthenticationException(new ConnectionId(_serverId), "test message"));

            using (var subject = CreateSubject())
            {
                _mockConnectionFactory
                    .Setup(f => f.CreateConnection(_serverId, _endPoint))
                    .Returns(() =>
                    {
                        subject._maintenanceCancellationTokenSource().Cancel(); // Task.Delay will be canceled 
                        return authenticationFailedConnection.Object;
                    });

                var _ = Record.Exception(() => subject.MaintainSizeAsync().GetAwaiter().GetResult());
                authenticationFailedConnection.Verify(conn => conn.Dispose(), Times.Once);
            }
        }

        [Fact]
        public void MaintainSizeAsync_should_not_try_new_attempt_after_failing_without_delay()
        {
            var settings = _settings.With(maintenanceInterval: TimeSpan.FromSeconds(10));

            using (var subject = CreateSubject(settings))
            {
                _mockConnectionFactory
                    .SetupSequence(f => f.CreateConnection(_serverId, _endPoint))
                    .Throws<Exception>()    // failed attempt
                    .Returns(() =>          // successful attempt which should be delayed
                    {
                        // break the loop. With this line the MaintainSizeAsync will contain only 2 iterations
                        subject._maintenanceCancellationTokenSource().Cancel();
                        return new MockConnection(_serverId);
                    });

                var testResult = Task.WaitAny(
                    subject.MaintainSizeAsync(),            // if this task is completed first, it will mean that there was no delay (10 sec) 
                    Task.Delay(TimeSpan.FromSeconds(1)));   // time to be sure that delay is happening,
                                                            // if the method is running more than 1 second, then delay is happening
                testResult.Should().Be(1);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void CheckoutOut_maxConnecting_is_enforced(
            [Values(true, false)]
            bool isAsync,
            [Values(1, 2, 4)]
            int maxConnecting)
        {
            int threadsCount = maxConnecting + 2;
            int maxConnectingCurrent = 0;

            var connectionOpenInFlightEvent = new CountdownEvent(maxConnecting);
            var unblockEvent = new ManualResetEventSlim(false);

            var settings = _settings.With(
                waitQueueSize: threadsCount,
                maxConnecting: maxConnecting,
                maxConnections: threadsCount,
                waitQueueTimeout: TimeSpan.FromSeconds(10),
                minConnections: 0);

            var mockConnectionFactory = new Mock<IConnectionFactory>();
            mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();
                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                        .Setup(c => c.Open(It.IsAny<CancellationToken>()))
                        .Callback(BlockConnectionOpen);

                    connectionMock
                        .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                        .Returns(() =>
                        {
                            BlockConnectionOpen();
                            return Task.FromResult(0);
                        });

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();

            ExecuteOnNewThread(threadsCount + 1, i =>
            {
                if (i == threadsCount)
                {
                    // wait until open-connection in flight achieved maxConnecting
                    connectionOpenInFlightEvent.Wait();

                    // wait until all threads try to acquire connections
                    do
                    {
                        Thread.Sleep(100);
                    } while (subject.AvailableCount > 0);

                    // unblock all open-connection in flight
                    unblockEvent.Set();
                }
                else
                {
                    AcquireConnectionGeneric(subject, isAsync).Should().NotBeNull();
                }
            });

            void BlockConnectionOpen()
            {
                // validate maxConnecting is respected
                Interlocked.Increment(ref maxConnectingCurrent);
                maxConnectingCurrent.Should().BeLessOrEqualTo(maxConnecting);

                // notify connection in flight
                if (connectionOpenInFlightEvent.CurrentCount > 0)
                    connectionOpenInFlightEvent.Signal();

                unblockEvent.Wait();

                Interlocked.Decrement(ref maxConnectingCurrent);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void CheckoutOut_maxConnecting_timeout(
            [Values(true, false)]bool isAsync,
            [Values(1, 2)]int excessConnectingCount)
        {
            int maxConnecting = 2;
            int threadsCount = maxConnecting + excessConnectingCount;

            var settings = _settings.With(
                waitQueueSize: threadsCount,
                maxConnecting: maxConnecting,
                maxConnections: threadsCount,
                waitQueueTimeout: TimeSpan.FromMilliseconds(50),
                minConnections: 0);

            var blockedCountEvent = new CountdownEvent(maxConnecting);
            var unblockCountEvent = new CountdownEvent(excessConnectingCount);

            var mockConnectionFactory = new Mock<IConnectionFactory>();
            mockConnectionFactory
                .Setup(c => c.CreateConnection(It.IsAny<ServerId>(), It.IsAny<EndPoint>()))
                .Returns(() =>
                {
                    var connectionMock = new Mock<IConnection>();
                    connectionMock
                        .Setup(c => c.Settings)
                        .Returns(new ConnectionSettings());

                    connectionMock
                        .Setup(c => c.Open(It.IsAny<CancellationToken>()))
                        .Callback(() =>
                        {
                            if (blockedCountEvent.CurrentCount > 0)
                            {
                                blockedCountEvent.Signal();
                            }

                            unblockCountEvent.Wait();
                        });

                    connectionMock
                        .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                        .Returns(() =>
                        {
                            if (blockedCountEvent.CurrentCount > 0)
                            {
                                blockedCountEvent.Signal();
                            }

                            unblockCountEvent.Wait();

                            return Task.FromResult(0);
                        });

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();

            ExecuteOnNewThread(threadsCount, i =>
            {
                if (i >= maxConnecting)
                {
                    blockedCountEvent.Wait();

                    Record.Exception(() => AcquireConnectionGeneric(subject, isAsync))
                        .Should()
                        .BeOfType<TimeoutException>();

                    unblockCountEvent.Signal();
                }
                else
                {
                    AcquireConnectionGeneric(subject, isAsync);
                }
            });
        }

        [Theory]
        [ParameterAttributeData]
        public void CheckoutOut_returned_connection_maxConnecting(
            [Values(true, false)] bool isAsync,
            [Values(1, 2)]int excessConnectingCount)
        {
            int maxConnecting = 2;
            int initalAcquiredCount = excessConnectingCount;
            int threadsCount = maxConnecting + excessConnectingCount;

            var settings = _settings.With(
                waitQueueSize: threadsCount,
                maxConnecting: maxConnecting,
                maxConnections: maxConnecting + excessConnectingCount + initalAcquiredCount,
                waitQueueTimeout: TimeSpan.FromSeconds(200),
                minConnections: 0);

            var allAcquiringCountEvent = new CountdownEvent(initalAcquiredCount + maxConnecting);
            var unblockEvent = new ManualResetEventSlim(true);

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
                            if (allAcquiringCountEvent.CurrentCount > 0)
                            {
                                allAcquiringCountEvent.Signal();
                            }

                            unblockEvent.Wait();
                        });

                    connectionMock
                        .Setup(c => c.OpenAsync(It.IsAny<CancellationToken>()))
                        .Returns(() =>
                        {
                            if (allAcquiringCountEvent.CurrentCount > 0)
                            {
                                allAcquiringCountEvent.Signal();
                            }
                            unblockEvent.Wait();

                            return Task.FromResult(0);
                        });

                    return connectionMock.Object;
                });

            using var subject = CreateSubject(settings, mockConnectionFactory.Object);
            subject.Initialize();

            var connectionsAcquired = Enumerable.Range(0, initalAcquiredCount)
                .Select(i => AcquireConnectionGeneric(subject, isAsync))
                .ToArray();

            var connectionsReused = new ConcurrentBag<ConnectionId>();

            // block connections establishments
            unblockEvent.Reset();

            // schedule connections acquirement
            ExecuteOnNewThread(threadsCount + 1, i =>
            {
                if (i == threadsCount)
                {
                    // orchestrator thread
                    // wait until all trying to connect
                    do
                    {
                        Thread.Sleep(100);
                    }
                    while (subject.AvailableCount > 0);

                    // return connections and unblock acquirements waiting for establishment
                    foreach (var connection in connectionsAcquired)
                    {
                        connection.Dispose();
                    }
                }
                else if (i >= maxConnecting)
                {
                    // wait until all establishments are in-flight
                    allAcquiringCountEvent.Wait();

                    // acquire reused connection
                    var connection = AcquireConnectionGeneric(subject, isAsync);
                    connectionsReused.Add(connection.ConnectionId);

                    if (connectionsReused.Count == excessConnectingCount)
                    {
                        // unblock maxConnecting for clean up
                        unblockEvent.Set();
                    }
                }
                else
                {
                    // maximize establishments in-flight up to maxConnecting
                    AcquireConnectionGeneric(subject, isAsync);
                }
            });

            connectionsAcquired.Length.Should().Be(connectionsReused.Count);
            foreach (var connectionAcquired in connectionsAcquired)
            {
                connectionsReused.Contains(connectionAcquired.ConnectionId);
            }
        }

        // private methods
        // TODO BD, use ThreadingUtilities from BsonTests.Helper
        private static void ExecuteOnNewThread(int threadsCount, Action<int> action, int timeoutMilliseconds = 100000)
        {
            var actionsExecutedCount = 0;

            var exceptions = new ConcurrentBag<Exception>();

            var threads = Enumerable.Range(0, threadsCount).Select(i =>
            {
                var thread = new Thread(_ =>
                {
                    try
                    {
                        action(i);
                        Interlocked.Increment(ref actionsExecutedCount);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });

                thread.Start();

                return thread;
            })
            .ToArray();

            foreach (var thread in threads)
            {
                if (!thread.Join(timeoutMilliseconds))
                {
                    throw new TimeoutException();
                }
            }

            if (exceptions.Any())
            {
                throw exceptions.First();
            }

            actionsExecutedCount.Should().Be(threadsCount);
        }

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
                _capturedEvents);
        }

        private void InitializeAndWait()
        {
            _subject.Initialize();

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
        public static CancellationTokenSource _maintenanceCancellationTokenSource(this ExclusiveConnectionPool obj)
        {
            return (CancellationTokenSource)Reflector.GetFieldValue(obj, nameof(_maintenanceCancellationTokenSource));
        }

        public static Task MaintainSizeAsync(this ExclusiveConnectionPool obj)
        {
            return (Task)Reflector.Invoke(obj, nameof(MaintainSizeAsync));
        }
    }
}
