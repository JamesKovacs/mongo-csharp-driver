﻿/* Copyright 2010-present MongoDB Inc.
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Authentication.External;
using MongoDB.Driver.Core.Authentication.Sasl;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication.Oidc
{
    internal abstract class OidcSaslMechanism : SaslMechanismBase
    {
        protected static ISaslStep CreateLastSaslStep(IExternalCredentials oidcCredentials)
        {
            if (oidcCredentials == null)
            {
                throw new InvalidOperationException("OIDC credentials have not been provided.");
            }
            return new NoTransitionClientLast(new BsonDocument("jwt", oidcCredentials.AccessToken).ToBson());
        }

        public abstract ICredentialsCache<OidcCredentials> CredentialsCache { get; }

        public override string Name => MongoOidcAuthenticator.MechanismName;

        public abstract ISaslStep CreateSpeculativeAuthenticationSaslStep(CancellationToken cancellationToken);
        public virtual Task<ISaslStep> CreateSpeculativeAuthenticationSaslStepAsync(CancellationToken cancellationToken) => Task.FromResult(CreateSpeculativeAuthenticationSaslStep(cancellationToken));
        public abstract bool ShouldReauthenticateIfSaslError(IConnection connection, Exception ex);
    }

    /// <summary>
    /// The Mongo OIDC authenticator.
    /// </summary>
    internal sealed class MongoOidcAuthenticator : SaslAuthenticator
    {
        #region static
        public const string AllowedHostsMechanismProperyName = "ALLOWED_HOSTS";
        public const string ProviderMechanismProperyName = "PROVIDER_NAME";
        public const string MechanismName = "MONGODB-OIDC";
        public const string RequestCallbackMechanismProperyName = "REQUEST_TOKEN_CALLBACK";
        public const string RefreshCallbackMechanismProperyName = "REFRESH_TOKEN_CALLBACK";

        public static readonly IEnumerable<string> DefaultAllowedHostNames = new[] { "*.mongodb.net", "*.mongodb-dev.net", "*.mongodbgov.net", "localhost", "::1" };

        /// <summary>
        /// Create OIDC authenticator.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="principalName">The principalName.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="endPoint">The endpoint.</param>
        /// <param name="serverApi">The server API.</param>
        /// <returns>The oidc authenticator.</returns>
        public static MongoOidcAuthenticator CreateAuthenticator(
            string source,
            string principalName,
            IEnumerable<KeyValuePair<string, string>> properties,
            EndPoint endPoint,
            ServerApi serverApi) =>
            CreateAuthenticator(source, principalName, properties, endPoint, serverApi, ExternalCredentialsAuthenticators.Instance);

        internal static MongoOidcAuthenticator CreateAuthenticator(
            string source,
            string principalName,
            IEnumerable<KeyValuePair<string, string>> properties,
            EndPoint endpoint,
            ServerApi serverApi,
            IExternalCredentialsAuthenticators externalCredentialsAuthenticators) =>
        CreateAuthenticator(
            source,
            principalName,
            properties.Select(pair => new KeyValuePair<string, object>(pair.Key, pair.Value)),
            endpoint,
            serverApi,
            externalCredentialsAuthenticators);

        /// <summary>
        /// Create OIDC authenticator.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="principalName">The principal name.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="endpoint">The current endpoint.</param>
        /// <param name="serverApi">The server API.</param>
        /// <returns>The oidc authenticator.</returns>
        public static MongoOidcAuthenticator CreateAuthenticator(
            string source,
            string principalName,
            IEnumerable<KeyValuePair<string, object>> properties,
            EndPoint endpoint,
            ServerApi serverApi) =>
            CreateAuthenticator(source, principalName, properties, endpoint, serverApi, ExternalCredentialsAuthenticators.Instance);

        internal static MongoOidcAuthenticator CreateAuthenticator(
            string source,
            string principalName,
            IEnumerable<KeyValuePair<string, object>> properties,
            EndPoint endpoint,
            ServerApi serverApi,
            IExternalCredentialsAuthenticators externalCredentialsAuthenticators)
        {
            Ensure.IsNotNull(endpoint, nameof(endpoint));
            Ensure.IsNotNull(externalCredentialsAuthenticators, nameof(externalCredentialsAuthenticators));

            if (source != "$external")
            {
                throw new ArgumentException("MONGODB-OIDC authentication may only use the $external source.", nameof(source));
            }

            var inputConfiguration = CreateInputConfiguration(endpoint, principalName, properties);

            OidcSaslMechanism mechanism;
            if (inputConfiguration.IsCallbackWorkflow)
            {
                var oidcAuthenticator = externalCredentialsAuthenticators.Oidc;
                var oidsCredentialsProvider = oidcAuthenticator.GetProvider(inputConfiguration);
                var oidcTimeSynchronizerContext = new OidcTimeSynchronizerContext(oidcAuthenticator.TimeSynchronizer);
                mechanism = new MongoOidcCallbackMechanism(inputConfiguration.PrincipalName, oidsCredentialsProvider, oidcTimeSynchronizerContext);
            }
            else
            {
                var providerName = Ensure.IsNotNull(inputConfiguration.ProviderName, nameof(inputConfiguration.ProviderName));
                IExternalAuthenticationCredentialsProvider<OidcCredentials> provider = providerName switch
                {
                    "aws" => new OidcAuthenticationCredentialsProviderAdapter<OidcCredentials>(externalCredentialsAuthenticators.AwsForOidc),
                    "azure" => new OidcAuthenticationCredentialsProviderAdapter<AzureCredentials>(externalCredentialsAuthenticators.Azure),
                    "gcp" => new OidcAuthenticationCredentialsProviderAdapter<GcpCredentials>(externalCredentialsAuthenticators.Gcp),
                    _ => throw new NotSupportedException($"Not supported provider name: {providerName} for OIDC authentication.")
                };
                mechanism = new MongoOidcProviderMechanism(provider);
            }
            return new MongoOidcAuthenticator(mechanism, serverApi);

            static OidcInputConfiguration CreateInputConfiguration(
                EndPoint endpoint,
                string principalName,
                IEnumerable<KeyValuePair<string, object>> properties)
            {
                if (properties == null)
                {
                    return new OidcInputConfiguration(endpoint, principalName);
                }

                IEnumerable<string> allowedHostNames = DefaultAllowedHostNames;
                string providerName = null;
                IOidcRequestCallbackProvider requestCallbackProvider = null;
                IOidcRefreshCallbackProvider refreshCallbackProvider = null;
                foreach (var authorizationProperty in properties)
                {
                    var value = authorizationProperty.Value;
                    switch (authorizationProperty.Key)
                    {
                        case AllowedHostsMechanismProperyName:
                            {
                                allowedHostNames = value is IEnumerable<string> enumerable
                                    ? enumerable
                                    : throw new InvalidCastException($"The {AllowedHostsMechanismProperyName} must be array, but was {value.GetType()}.");
                            }
                            break;
                        case RequestCallbackMechanismProperyName:
                            {
                                requestCallbackProvider = value is IOidcRequestCallbackProvider requestProvider
                                    ? requestProvider
                                    : throw new InvalidCastException($"The {RequestCallbackMechanismProperyName} must be inherited from {nameof(IOidcRequestCallbackProvider)}, but was {value.GetType()}.");
                            }
                            break;
                        case RefreshCallbackMechanismProperyName:
                            {
                                refreshCallbackProvider = value is IOidcRefreshCallbackProvider refreshProvider
                                    ? refreshProvider
                                    : throw new InvalidCastException($"The {RefreshCallbackMechanismProperyName} must be inherited from {nameof(IOidcRefreshCallbackProvider)}, but was {value.GetType()}.");
                            }
                            break;
                        case ProviderMechanismProperyName:
                            {
                                providerName = value is string @string
                                    ? @string
                                    : throw new InvalidCastException($"The {ProviderMechanismProperyName} must be string, but was {value.GetType()}.");
                            }
                            break;
                        default: throw new ArgumentException($"Unknown OIDC property '{authorizationProperty.Key}'.", nameof(authorizationProperty));
                    }
                }

                EnsureHostsAreValid(endpoint, allowedHostNames);
                return new OidcInputConfiguration(endpoint, principalName, providerName, requestCallbackProvider, refreshCallbackProvider);
            }

            static IEnumerable<string> EnsureHostsAreValid(EndPoint endPoint, IEnumerable<string> allowedHosts)
            {
                var allowedHostsCount = Ensure.IsNotNull(allowedHosts, nameof(allowedHosts)).Count();
                if (allowedHostsCount == 0)
                {
                    throw new InvalidOperationException($"{nameof(AllowedHostsMechanismProperyName)} mechanism authentication property must contain at least one host.");
                }

                var host = EndPointHelper.GetHostAndPort(endPoint).Host;
                if (allowedHosts.Any(ah => IsHostMatch(host, ah)))
                {
                    return allowedHosts;
                }
                else
                {
                    throw new InvalidOperationException($"The used host '{host}' doesn't match allowed hosts list ['{string.Join("', '" , allowedHosts)}'].");
                }

                static bool IsHostMatch(string host, string pattern)
                {
                    if (pattern != null)
                    {
                        var index = pattern.IndexOf('*');
                        if (index != -1)
                        {
                            var filterPattern = pattern.Substring(index + 1);
                            if (filterPattern.Length > 0)
                            {
                                return host.EndsWith(filterPattern);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return pattern == host;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        #endregion

        private readonly new OidcSaslMechanism _mechanism;

        private MongoOidcAuthenticator(
            OidcSaslMechanism mechanism,
            ServerApi serverApi)
            : base(mechanism, serverApi)
        {
            _mechanism = Ensure.IsNotNull(mechanism, nameof(mechanism));
        }

        /// <summary>
        /// The database name.
        /// </summary>
        public override string DatabaseName => "$external";

        /// <inheritdoc/>
        public override void Authenticate(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            try
            {
                base.Authenticate(connection, description, cancellationToken);
            }
            catch (Exception ex)
            {
                ClearCredentials();
                if (_mechanism.ShouldReauthenticateIfSaslError(connection, ex))
                {
                    base.Authenticate(connection, description, cancellationToken);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override async Task AuthenticateAsync(IConnection connection, ConnectionDescription description, CancellationToken cancellationToken)
        {
            try
            {
                await base.AuthenticateAsync(connection, description, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                ClearCredentials();
                if (_mechanism.ShouldReauthenticateIfSaslError(connection, ex))
                {
                    await base.AuthenticateAsync(connection, description, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public override BsonDocument CustomizeInitialHelloCommand(BsonDocument helloCommand, CancellationToken cancellationToken)
        {
            _speculativeFirstStep = _mechanism.CreateSpeculativeAuthenticationSaslStep(cancellationToken);
            if (_speculativeFirstStep != null)
            {
                var firstCommand = CreateStartCommand(_speculativeFirstStep);
                firstCommand.Add("db", DatabaseName);
                helloCommand.Add("speculativeAuthenticate", firstCommand);
            }
            return helloCommand;
        }

        // protected methods
        private protected override MongoAuthenticationException CreateException(ConnectionId connectionId, Exception ex, BsonDocument command)
        {
            var originalException = base.CreateException(connectionId, ex, command);
            if (_mechanism is MongoOidcCallbackMechanism)
            {
                var payload = BsonSerializer.Deserialize<BsonDocument>(command["payload"].AsByteArray);
                // if no jwt, then cached credentials are not involved
                var allowReauthenticationAfterError = payload.Contains("jwt");
                return new MongoAuthenticationException(connectionId, originalException.Message, ex, allowReauthenticationAfterError);
            }
            else
            {
                return originalException;
            }
        }

        // private methods
        private void ClearCredentials() => _mechanism?.CredentialsCache.Clear();
    }
}
