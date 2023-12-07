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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver.Core.Authentication.Oidc
{
    internal class OidcSaslMechanism : ISaslMechanism
    {
        private readonly IOidcCallbackAdapter _oidcCallback;

        public OidcSaslMechanism(IOidcCallbackAdapter oidcCallback)
        {
            _oidcCallback = oidcCallback;
        }

        public string Name => MongoOidcAuthenticator.MechanismName;

        public ISaslStep Initialize(
            IConnection connection,
            SaslConversation conversation,
            ConnectionDescription description,
            CancellationToken cancellationToken)
            => StartAuthenticationFlow(default);

        public Task<ISaslStep> InitializeAsync(
            IConnection connection,
            SaslConversation conversation,
            ConnectionDescription description,
            CancellationToken cancellationToken)
            => StartAuthenticationFlowAsync(cancellationToken);

        public ISaslStep CreateSpeculativeAuthenticationStep(CancellationToken cancellationToken)
        {
            var cachedCredentials = _oidcCallback.CachedCredentials;
            if (cachedCredentials == null)
            {
                return null;
            }

            return CreateNoTransitionClientLastSaslStep(cachedCredentials);
        }

        public Task<ISaslStep> CreateSpeculativeAuthenticationStepAsync(CancellationToken cancellationToken)
            => Task.FromResult(CreateSpeculativeAuthenticationStep(cancellationToken));

        public void ClearCache() => _oidcCallback.ClearCache();

        private ISaslStep StartAuthenticationFlow(CancellationToken cancellationToken)
        {
            var credentials = _oidcCallback.GetCredentials(new OidcCallbackParameters(1), cancellationToken);
            return CreateNoTransitionClientLastSaslStep(credentials);
        }

        private async Task<ISaslStep> StartAuthenticationFlowAsync(CancellationToken cancellationToken)
        {
            var credentials =  await _oidcCallback.GetCredentialsAsync(new OidcCallbackParameters(1), cancellationToken);
            return CreateNoTransitionClientLastSaslStep(credentials);
        }

        private static ISaslStep CreateNoTransitionClientLastSaslStep(OidcCredentials oidcCredentials)
        {
            if (oidcCredentials == null)
            {
                throw new InvalidOperationException("OIDC credentials have not been provided.");
            }

            return new NoTransitionClientLastSaslStep(new BsonDocument("jwt", oidcCredentials.AccessToken).ToBson());
        }
    }
}
