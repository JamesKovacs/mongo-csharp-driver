﻿using System;
using System.Net;
using FluentAssertions;
using Xunit;
using PoolState = MongoDB.Driver.Core.ConnectionPools.ExclusiveConnectionPool.PoolState;
using State = MongoDB.Driver.Core.ConnectionPools.ExclusiveConnectionPool.State;

namespace MongoDB.Driver.Core.Tests.Core.ConnectionPools
{
    public class ExclusiveConnectionPoolStateTests
    {
        [Fact]
        public void PoolState_should_start_at_initial_state()
        {
            var poolState = new PoolState();
            poolState.State.Should().Be(State.Uninitialized);
        }

        [Theory]
        [InlineData(new[] { State.Disposed })]
        [InlineData(new[] { State.Disposed, State.Disposed })]
        [InlineData(new[] { State.Paused, State.Ready })]
        [InlineData(new[] { State.Paused, State.Paused })]
        [InlineData(new[] { State.Paused, State.Disposed })]
        [InlineData(new[] { State.Paused, State.Disposed, State.Disposed })]
        [InlineData(new[] { State.Paused, State.Ready, State.Disposed })]
        [InlineData(new[] { State.Paused, State.Ready, State.Ready })]
        [InlineData(new[] { State.Paused, State.Ready, State.Paused, State.Ready })]
        [InlineData(new[] { State.Paused, State.Ready, State.Paused, State.Ready, State.Disposed })]
        internal void PoolState_should_transition_on_valid_transitions(State[] states)
        {
            _ = CreatePoolStateAndValidate(states);
        }

        [Theory]
        [InlineData(null, State.Uninitialized)]
        [InlineData(null, State.Ready)]
        [InlineData(new[] { State.Paused }, State.Uninitialized)]
        [InlineData(new[] { State.Paused, State.Ready }, State.Uninitialized)]
        [InlineData(new[] { State.Disposed }, State.Uninitialized)]
        [InlineData(new[] { State.Disposed }, State.Ready)]
        [InlineData(new[] { State.Disposed }, State.Paused)]
        internal void PoolState_should_throw_on_invalid_transitions(State[] validStates, State invalidState)
        {
            var poolState = CreatePoolStateAndValidate(validStates);

            var exception = Record.Exception(() => poolState.TransitionState(invalidState));

            if (poolState.State == State.Disposed)
            {
                exception.Should().BeOfType<ObjectDisposedException>();
            }
            else
            {
                exception.Should().BeOfType<InvalidOperationException>();
            }
        }

        [Fact]
        internal void PoolState_ThrowIfDisposed_should_throw_when_disposed()
        {
            var poolState = CreatePoolStateAndValidate(State.Disposed);

            var exception = Record.Exception(() => poolState.ThrowIfDisposed());
            exception.Should().BeOfType<ObjectDisposedException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new[] { State.Paused })]
        [InlineData(new[] { State.Paused, State.Ready })]
        internal void PoolState_ThrowIfDisposed_should_not_throw_when_not_disposed(State[] states)
        {
            var poolState = CreatePoolStateAndValidate(states);
            poolState.ThrowIfDisposed();
        }

        [Theory]
        [InlineData(null)]
        [InlineData(new[] { State.Disposed })]
        [InlineData(new[] { State.Paused })]
        internal void PoolState_ThrowIfDisposedOrNotReady_should_throw_when_not_ready(State[] states)
        {
            var endpoint = new DnsEndPoint("host", 1234);
            var poolState = CreatePoolStateAndValidate(states);
            var exception = Record.Exception(() => poolState.ThrowIfDisposedOrNotReady(endpoint));

            switch (poolState.State)
            {
                case State.Disposed:
                    exception.Should().BeOfType<ObjectDisposedException>();
                    break;
                case State.Paused:
                    exception.Should().BeOfType<MongoConnectionPoolPausedException>();
                    break;
                default:
                    exception.Should().BeOfType<InvalidOperationException>();
                    break;
            }
        }

        [Fact]
        internal void PoolState_ThrowIfDisposedOrNotReady_should_not_throw_when_ready()
        {
            var endpoint = new DnsEndPoint("host", 1234);
            var poolState = CreatePoolStateAndValidate(State.Paused, State.Ready);
            poolState.ThrowIfDisposedOrNotReady(endpoint);
        }

        [Fact]
        internal void PoolState_ThrowIfNotInitialized_should_throw_when_not_initialized()
        {
            var poolState = CreatePoolStateAndValidate();
            var exception = Record.Exception(() => poolState.ThrowIfNotInitialized());
            exception.Should().BeOfType<InvalidOperationException>();
        }

        [Theory]
        [InlineData(new[] { State.Disposed })]
        [InlineData(new[] { State.Paused })]
        [InlineData(new[] { State.Paused, State.Ready })]
        internal void PoolState_ThrowIfNotInitialized_should_not_throw_when_initialized(State[] states)
        {
            var poolState = CreatePoolStateAndValidate(states);
            poolState.ThrowIfNotInitialized();
        }

        // private methods
        private PoolState CreatePoolStateAndValidate(params State[] states)
        {
            var poolState = new PoolState();

            if (states != null)
            {
                foreach (var state in states)
                {
                    poolState.TransitionState(state);
                    poolState.State.Should().Be(state);
                }
            }

            return poolState;
        }
    }
}
