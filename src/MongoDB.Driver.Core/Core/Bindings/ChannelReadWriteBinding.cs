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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a read-write binding that is bound to a channel.
    /// </summary>
    public sealed class ChannelReadWriteBinding : IReadWriteBinding
    {
        // fields
        private readonly IChannelHandle _channel;
        private bool _disposed;
        private readonly ReadPreference _effectiveReadPreference;
        private readonly IServer _server;
        private readonly ICoreSessionHandle _session;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelReadWriteBinding" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="session">The session.</param>
        public ChannelReadWriteBinding(IServer server, IChannelHandle channel, ICoreSessionHandle session)
            : this(server, channel, effectiveReadPreference: ReadPreference.Primary, session)
        {
            _server = Ensure.IsNotNull(server, nameof(server));
            _channel = Ensure.IsNotNull(channel, nameof(channel));
            _session = Ensure.IsNotNull(session, nameof(session));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelReadWriteBinding" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="channel">The channel.</param>
        /// <param name="effectiveReadPreference">The effective read preference.</param>
        /// <param name="session">The session.</param>
        public ChannelReadWriteBinding(IServer server, IChannelHandle channel, ReadPreference effectiveReadPreference, ICoreSessionHandle session)
        {
            _server = Ensure.IsNotNull(server, nameof(server));
            _channel = Ensure.IsNotNull(channel, nameof(channel));
            _effectiveReadPreference = Ensure.IsNotNull(effectiveReadPreference, nameof(effectiveReadPreference));
            _session = Ensure.IsNotNull(session, nameof(session));
        }

        // properties
        /// <inheritdoc/>
        public ReadPreference ReadPreference
        {
            get { return ReadPreference.Primary; }
        }

        /// <inheritdoc/>
        public ICoreSessionHandle Session
        {
            get { return _session; }
        }

        // methods
        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _channel.Dispose();
                _session.Dispose();
                _disposed = true;
            }
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return CreateChannelSource();
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(CreateChannelSource());
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return CreateChannelSource();
        }

        /// <inheritdoc/>
        public ChannelSourceAndEffectiveReadPreference GetWriteChannelSource(IMayUseSecondaryCriteria mayUseSecondary, CancellationToken cancellationToken)
        {
            var channelSource = CreateChannelSource();

            return new ChannelSourceAndEffectiveReadPreference
            {
                ChannelSource = channelSource,
                EffectiveReadPreference = _effectiveReadPreference
            };
        }

        /// <inheritdoc/>
        public Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            return Task.FromResult(CreateChannelSource());
        }

        /// <inheritdoc/>
        public Task<ChannelSourceAndEffectiveReadPreference> GetWriteChannelSourceAsync(IMayUseSecondaryCriteria mayUseSecondary, CancellationToken cancellationToken)
        {
            var channelSource = CreateChannelSource();

            return Task.FromResult(
                new ChannelSourceAndEffectiveReadPreference
                {
                    ChannelSource = channelSource,
                    EffectiveReadPreference = _effectiveReadPreference
                }
            );
        }

        // private methods
        private IChannelSourceHandle CreateChannelSource()
        {
            return new ChannelSourceHandle(new ChannelChannelSource(_server, _channel.Fork(), _session.Fork()));
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
