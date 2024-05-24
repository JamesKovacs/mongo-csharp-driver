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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Clusters.ServerSelectors;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Bindings
{
    /// <summary>
    /// Represents a write binding to a writable server.
    /// </summary>
    public sealed class WritableServerBinding : IReadWriteBinding
    {
        // fields
        private readonly ICluster _cluster;
        private bool _disposed;
        private readonly ICoreSessionHandle _session;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="WritableServerBinding" /> class.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="session">The session.</param>
        public WritableServerBinding(ICluster cluster, ICoreSessionHandle session)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
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
        public IChannelSourceHandle GetReadChannelSource(CancellationToken cancellationToken)
        {
            return GetReadChannelSource(null, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetReadChannelSourceAsync(CancellationToken cancellationToken)
        {
            return await GetReadChannelSourceAsync(null, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public IChannelSourceHandle GetReadChannelSource(IReadOnlyCollection<ServerDescription> deprioritizedServers, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = _cluster.SelectServerAndPinIfNeeded(_session, WritableServerSelector.Instance, deprioritizedServers, cancellationToken);
            return CreateServerChannelSource(server);
        }

        /// <inheritdoc />
        public async Task<IChannelSourceHandle> GetReadChannelSourceAsync(IReadOnlyCollection<ServerDescription> deprioritizedServers, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = await _cluster.SelectServerAndPinIfNeededAsync(_session, WritableServerSelector.Instance, deprioritizedServers, cancellationToken).ConfigureAwait(false);
            return CreateServerChannelSource(server);
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetWriteChannelSource(CancellationToken cancellationToken)
        {
            return GetWriteChannelSource(deprioritizedServers: null, cancellationToken);
        }

        /// <inheritdoc />
        public IChannelSourceHandle GetWriteChannelSource(IReadOnlyCollection<ServerDescription> deprioritizedServers, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = _cluster.SelectServerAndPinIfNeeded(_session, WritableServerSelector.Instance, deprioritizedServers, cancellationToken);
            return CreateServerChannelSource(server);
        }

        /// <inheritdoc/>
        public IChannelSourceHandle GetWriteChannelSource(IMayUseSecondaryCriteria mayUseSecondary, CancellationToken cancellationToken)
        {
            return GetWriteChannelSource(null, mayUseSecondary, cancellationToken);
        }

        /// <inheritdoc />
        public IChannelSourceHandle GetWriteChannelSource(IReadOnlyCollection<ServerDescription> deprioritizedServers, IMayUseSecondaryCriteria mayUseSecondary, CancellationToken cancellationToken)
        {
            if (IsSessionPinnedToServer())
            {
                throw new InvalidOperationException($"This overload of {nameof(GetWriteChannelSource)} cannot be called when pinned to a server.");
            }

            var writableServerSelector = new WritableServerSelector(mayUseSecondary);

            var selector = deprioritizedServers != null
                ? (IServerSelector)new CompositeServerSelector(new IServerSelector[] { new PriorityServerSelector(deprioritizedServers), writableServerSelector })
                : writableServerSelector;

            var server = _cluster.SelectServer(selector, cancellationToken);
            return CreateServerChannelSource(server);
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetWriteChannelSourceAsync(CancellationToken cancellationToken)
        {
            return await GetWriteChannelSourceAsync(deprioritizedServers: null, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IChannelSourceHandle> GetWriteChannelSourceAsync(IReadOnlyCollection<ServerDescription> deprioritizedServers, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            var server = await _cluster.SelectServerAndPinIfNeededAsync(_session, WritableServerSelector.Instance, deprioritizedServers, cancellationToken).ConfigureAwait(false);
            return CreateServerChannelSource(server);
        }

        /// <inheritdoc/>
        public async Task<IChannelSourceHandle> GetWriteChannelSourceAsync(IMayUseSecondaryCriteria mayUseSecondary, CancellationToken cancellationToken)
        {
            return await GetWriteChannelSourceAsync(null, mayUseSecondary, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IChannelSourceHandle> GetWriteChannelSourceAsync(IReadOnlyCollection<ServerDescription> deprioritizedServers, IMayUseSecondaryCriteria mayUseSecondary, CancellationToken cancellationToken)
        {
            if (IsSessionPinnedToServer())
            {
                throw new InvalidOperationException($"This overload of {nameof(GetWriteChannelSource)} cannot be called when pinned to a server.");
            }

            var writableServerSelector = new WritableServerSelector(mayUseSecondary);

            IServerSelector selector = deprioritizedServers != null
                ? new CompositeServerSelector(new IServerSelector[] { new PriorityServerSelector(deprioritizedServers), writableServerSelector })
                : writableServerSelector;

            var server = await _cluster.SelectServerAsync(selector, cancellationToken).ConfigureAwait(false);
            return CreateServerChannelSource(server);
        }

        private IChannelSourceHandle CreateServerChannelSource(IServer server)
        {
            return new ChannelSourceHandle(new ServerChannelSource(server, _session.Fork()));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _session.Dispose();
                _disposed = true;
            }
        }

        private bool IsSessionPinnedToServer()
        {
            return _session.IsInTransaction && _session.CurrentTransaction.PinnedServer != null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
