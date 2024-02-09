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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication.Oidc
{
    /// <summary>
    /// The Mongo OIDC authenticator.
    /// </summary>
    internal sealed class MongoOidcAuthenticator : SaslAuthenticator
    {
        #region static
        public const string CallbackMechanismPropertyName = "OIDC_CALLBACK";
        public const string MechanismName = "MONGODB-OIDC";
        public const string ProviderMechanismPropertyName = "PROVIDER_NAME";

        /// <summary>
        /// Create OIDC authenticator.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="principalName">The principal name.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="context">The authentication context.</param>
        /// <param name="serverApi">The server API.</param>
        /// <returns>The oidc authenticator.</returns>
        public static MongoOidcAuthenticator CreateAuthenticator(
            string source,
            string principalName,
            IEnumerable<KeyValuePair<string, object>> properties,
            IAuthenticationContext context,
            ServerApi serverApi)
            => CreateAuthenticator(
                source,
                principalName,
                properties,
                context,
                serverApi,
                OidcCallbackAdapterCachingFactory.Instance,
                OidcCredentialsProviders.Instance);

        /// <summary>
        /// Create OIDC authenticator by explicitly providing inner components, used for tests mostly.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="principalName">The principal name.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="context">The authentication context.</param>
        /// <param name="serverApi">The server API.</param>
        /// <param name="callbackAdapterFactory">Oidc callback adapter factory to use, OidcCallbackAdapterCachingFactory is used by default</param>
        /// <param name="oidcCredentialsProviders">Oidc credentials providers</param>
        /// <returns>The oidc authenticator.</returns>
        internal static MongoOidcAuthenticator CreateAuthenticator(
            string source,
            string principalName,
            IEnumerable<KeyValuePair<string, object>> properties,
            IAuthenticationContext context,
            ServerApi serverApi,
            IOidcCallbackAdapterFactory callbackAdapterFactory,
            IOidcCredentialsProviders oidcCredentialsProviders)
        {
            Ensure.IsNotNull(context, nameof(context));
            var endPoint = Ensure.IsNotNull(context.CurrentEndPoint, nameof(context.CurrentEndPoint));
            Ensure.IsNotNull(callbackAdapterFactory, nameof(callbackAdapterFactory));
            Ensure.IsNotNull(oidcCredentialsProviders, nameof(oidcCredentialsProviders));

            if (source != "$external")
            {
                throw new ArgumentException("MONGODB-OIDC authentication may only use the $external source.", nameof(source));
            }

            var configuration = new OidcConfiguration(endPoint, principalName, properties, oidcCredentialsProviders);
            var callbackAdapter = callbackAdapterFactory.Get(configuration);
            var mechanism = new OidcSaslMechanism(callbackAdapter);
            return new MongoOidcAuthenticator(mechanism, serverApi, configuration);
        }
        #endregion

        internal MongoOidcAuthenticator(
            OidcSaslMechanism mechanism,
            ServerApi serverApi,
            OidcConfiguration configuration)
            : base(mechanism, serverApi)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// The database name.
        /// </summary>
        public override string DatabaseName => "$external";

        public OidcConfiguration Configuration { get; }

        private OidcSaslMechanism OidcMechanism => (OidcSaslMechanism)_mechanism;

        /// <inheritdoc/>
        public override void Authenticate(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            // Capture the cache state to decide if we want retry on auth error or not.
            // Not the best solution, but let us not to introduce the retry logic into SaslAuthenticator to reduce affected areas for now.
            // Consider to move this code into SaslAuthenticator when retry logic will be applicable not only for Oidc Auth.
            var allowRetryOnAuthError = OidcMechanism.HasCachedCredentials();
            try
            {
                base.Authenticate(connection, description, cancellationToken);
            }
            catch (MongoAuthenticationException authenticationException) when (allowRetryOnAuthError && ShouldReauthenticateIfSaslError(authenticationException, connection))
            {
                ClearCredentialsCache();
                Thread.Sleep(100);
                try
                {
                    base.Authenticate(connection, description, cancellationToken);
                }
                catch (Exception ex)
                {
                    ClearCredentialsCache();
                    throw UnwrapMongoAuthenticationException(ex);
                }
            }
            catch (Exception ex)
            {
                ClearCredentialsCache();
                throw UnwrapMongoAuthenticationException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task AuthenticateAsync(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            // Capture the cache state to decide if we want retry on auth error or not.
            // Not the best solution, but let us not to introduce the retry logic into SaslAuthenticator to reduce affected areas for now.
            // Consider to move this code into SaslAuthenticator when retry logic will be applicable not only for Oidc Auth.
            var allowRetryOnAuthError = OidcMechanism.HasCachedCredentials();
            try
            {
                await base.AuthenticateAsync(connection, description, cancellationToken).ConfigureAwait(false);
            }
            catch (MongoAuthenticationException authenticationException) when (allowRetryOnAuthError && ShouldReauthenticateIfSaslError(authenticationException, connection))
            {
                ClearCredentialsCache();
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                try
                {
                    await base.AuthenticateAsync(connection, description, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    ClearCredentialsCache();
                    throw UnwrapMongoAuthenticationException(ex);
                }
            }
            catch (Exception ex)
            {
                ClearCredentialsCache();
                throw UnwrapMongoAuthenticationException(ex);
            }
        }

        /// <inheritdoc/>
        public override BsonDocument CustomizeInitialHelloCommand(BsonDocument helloCommand, CancellationToken cancellationToken)
        {
            _speculativeFirstStep = OidcMechanism.CreateSpeculativeAuthenticationStep(cancellationToken);
            if (_speculativeFirstStep != null)
            {
                var firstCommand = CreateStartCommand(_speculativeFirstStep);
                firstCommand.Add("db", DatabaseName);
                helloCommand.Add("speculativeAuthenticate", firstCommand);
            }
            return helloCommand;
        }

        public override async Task<BsonDocument> CustomizeInitialHelloCommandAsync(BsonDocument helloCommand, CancellationToken cancellationToken)
        {
            _speculativeFirstStep = await OidcMechanism.CreateSpeculativeAuthenticationStepAsync(cancellationToken).ConfigureAwait(false);
            if (_speculativeFirstStep != null)
            {
                var firstCommand = CreateStartCommand(_speculativeFirstStep);
                firstCommand.Add("db", DatabaseName);
                helloCommand.Add("speculativeAuthenticate", firstCommand);
            }
            return helloCommand;
        }

        public void ClearCredentialsCache()
            => OidcMechanism.ClearCache();

        private static bool ShouldReauthenticateIfSaslError(MongoAuthenticationException ex, IConnection connection)
        {
            return ex.InnerException is MongoCommandException mongoCommandException
                   && mongoCommandException.Code == (int)ServerErrorCode.AuthenticationFailed
                   && !connection.IsInitialized;
        }

        private static Exception UnwrapMongoAuthenticationException(Exception ex)
        {
            if (ex is MongoAuthenticationException mongoAuthenticationException
                && mongoAuthenticationException.InnerException != null)
            {
                return mongoAuthenticationException.InnerException;
            }

            return ex;
        }
    }
}
