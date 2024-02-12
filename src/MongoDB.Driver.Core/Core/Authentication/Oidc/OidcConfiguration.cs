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
using System.Net;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Authentication.Oidc
{
    internal sealed class OidcConfiguration
    {
        public OidcConfiguration(
            EndPoint endpoint,
            string principalName,
            IEnumerable<KeyValuePair<string, object>> properties,
            IOidcCredentialsProviders oidcCredentialsProviders)
        {
            EndPoint = Ensure.IsNotNull(endpoint, nameof(endpoint));
            PrincipalName = principalName;

            if (properties != null)
            {
                foreach (var authorizationProperty in properties)
                {
                    switch (authorizationProperty.Key)
                    {
                        case MongoOidcAuthenticator.CallbackMechanismPropertyName:
                            {
                                Callback = GetProperty<IOidcCallbackProvider>(authorizationProperty);
                            }
                            break;
                        case MongoOidcAuthenticator.ProviderMechanismPropertyName:
                            {
                                ProviderName = GetProperty<string>(authorizationProperty);
                            }
                            break;
                        default:
                            throw new ArgumentException($"Unknown OIDC property '{authorizationProperty.Key}'.",
                                nameof(authorizationProperty));
                    }
                }
            }

            EnsureOptionsValid();

            if (!string.IsNullOrEmpty(ProviderName))
            {
                Callback = ProviderName switch
                {
                    "aws" => oidcCredentialsProviders.Aws,
                    _ => throw new InvalidOperationException($"Not supported value of {MongoOidcAuthenticator.ProviderMechanismPropertyName} mechanism property: {ProviderName}.")
                };
            }
            return;

            static T GetProperty<T>(KeyValuePair<string, object> property)
            {
                if (property.Value is T result)
                {
                    return result;
                }

                throw new InvalidCastException($"Cannot read {property.Key} property as {typeof(T).Name}");
            }
        }

        public EndPoint EndPoint { get; }
        public string PrincipalName { get; }
        public string ProviderName { get; }
        public IOidcCallbackProvider Callback { get; }

        // public methods
        public override int GetHashCode() =>
            new Hasher()
                .Hash(ProviderName)
                .Hash(EndPoint)
                .Hash(PrincipalName)
                .GetHashCode();

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null) || GetType() != obj.GetType()) { return false; }
            var rhs = (OidcConfiguration)obj;
            return
                // This class is used as an oidc cache key for a callback workflow.
                ProviderName == rhs.ProviderName &&
                PrincipalName == rhs.PrincipalName &&
                object.Equals(Callback, rhs.Callback) &&
                EndPointHelper.Equals(EndPoint, rhs.EndPoint);
        }

        private void EnsureOptionsValid()
        {
            if (string.IsNullOrEmpty(ProviderName) && Callback == null)
            {
                throw new InvalidOperationException($"{MongoOidcAuthenticator.ProviderMechanismPropertyName} or {MongoOidcAuthenticator.CallbackMechanismPropertyName} must be configured.");
            }

            if (!string.IsNullOrEmpty(ProviderName) && Callback != null)
            {
                throw new InvalidOperationException($"{MongoOidcAuthenticator.CallbackMechanismPropertyName} is mutually exclusive with {MongoOidcAuthenticator.ProviderMechanismPropertyName}.");
            }
        }
    }
}
