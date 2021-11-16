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
    /// Represents a read/write binding to a single server.
    /// </summary>
    public sealed class SingleServerReadWriteBinding : IReadWriteBinding
    {
        // fields
        private bool _disposed;
        private readonly ReadPreference _effectiveReadPreference;
        private readonly IServer _server;
        private readonly ICoreSessionHandle _session;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleServerReadWriteBinding" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="session">The session.</param>
        public SingleServerReadWriteBinding(IServer server, ICoreSessionHandle session)
            : this(server, session, effectiveReadPreference: ReadPreference.Primary)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleServerReadWriteBinding" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="session">The session.</param>
        /// <param name="effectiveReadPreference">The effective read preference.</param>
        public SingleServerReadWriteBinding(IServer server, ICoreSessionHandle session, ReadPreference effectiveReadPreference)
        {
            _server = Ensure.IsNotNull(server, nameof(server));
            _session = Ensure.IsNotNull(session, nameof(session));
            _effectiveReadPreference = Ensure.IsNotNull(effectiveReadPreference, nameof(effectiveReadPreference));
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

        private IChannelSourceHandle CreateChannelSource()
        {
            return new ChannelSourceHandle(new ServerChannelSource(_server, _session.Fork()));
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
