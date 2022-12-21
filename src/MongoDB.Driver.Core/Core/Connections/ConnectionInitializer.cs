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
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents a connection initializer (opens and authenticates connections).
    /// </summary>
    internal class ConnectionInitializer : IConnectionInitializer
    {
        private readonly BsonDocument _clientDocument;
        private readonly IReadOnlyList<CompressorConfiguration> _compressors;
        private readonly ServerApi _serverApi;

        public ConnectionInitializer(
            string applicationName,
            IReadOnlyList<CompressorConfiguration> compressors,
            ServerApi serverApi)
        {
            _clientDocument = ClientDocumentHelper.CreateClientDocument(applicationName);
            _compressors = Ensure.IsNotNull(compressors, nameof(compressors));
            _serverApi = serverApi;
        }

        public ConnectionDescription Authenticate(IConnection connection, ConnectionInitializerContext connectionInitializerContext, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(connectionInitializerContext, nameof(connectionInitializerContext));
            var authenticators = connectionInitializerContext.Authenticators ?? GetAuthenticators(connection.EndPoint, connection.Settings);
            var description = Ensure.IsNotNull(connectionInitializerContext.Description, nameof(connectionInitializerContext.Description));

            AuthenticationHelper.Authenticate(connection, description, authenticators, cancellationToken);

            if (!description.ConnectionId.LongServerValue.HasValue)
            {
                var connectionIdServerValue = description.HelloResult.ConnectionIdServerValue;
                if (connectionIdServerValue.HasValue)
                {
                    description = UpdateConnectionIdWithServerValue(description, connectionIdServerValue.Value);
                }
                else if (!description.HelloResult.IsMongocryptd) // mongocryptd doesn't provide ConnectionId
                {
                    try
                    {
                        var getLastErrorProtocol = CreateGetLastErrorProtocol(_serverApi);
                        var getLastErrorResult = getLastErrorProtocol.Execute(connection, cancellationToken);

                        description = UpdateConnectionIdWithServerValue(description, getLastErrorResult);
                    }
                    catch
                    {
                        // if we couldn't get the server's connection id, so be it.
                    }
                }
            }

            return description;
        }

        public async Task<ConnectionDescription> AuthenticateAsync(IConnection connection, ConnectionInitializerContext connectionInitializerContext, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            Ensure.IsNotNull(connectionInitializerContext, nameof(connectionInitializerContext));
            var authenticators = connectionInitializerContext.Authenticators ?? GetAuthenticators(connection.EndPoint, connection.Settings);
            var description = Ensure.IsNotNull(connectionInitializerContext.Description, nameof(connectionInitializerContext.Description));

            await AuthenticationHelper.AuthenticateAsync(connection, description, authenticators, cancellationToken).ConfigureAwait(false);

            if (!description.ConnectionId.LongServerValue.HasValue)
            {
                var connectionIdServerValue = description.HelloResult.ConnectionIdServerValue;
                if (connectionIdServerValue.HasValue)
                {
                    description = UpdateConnectionIdWithServerValue(description, connectionIdServerValue.Value);
                }
                else if (!description.HelloResult.IsMongocryptd) // mongocryptd doesn't provide ConnectionId
                {
                    try
                    {
                        var getLastErrorProtocol = CreateGetLastErrorProtocol(_serverApi);
                        var getLastErrorResult = await getLastErrorProtocol
                            .ExecuteAsync(connection, cancellationToken)
                            .ConfigureAwait(false);

                        description = UpdateConnectionIdWithServerValue(description, getLastErrorResult);
                    }
                    catch
                    {
                        // if we couldn't get the server's connection id, so be it.
                    }
                }
            }

            return description;
        }

        public ConnectionInitializerContext SendHello(IConnection connection, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            var authenticators = GetAuthenticators(connection.EndPoint, connection.Settings);
            var helloCommand = CreateInitialHelloCommand(authenticators, connection.Settings.LoadBalanced, cancellationToken);
            var helloProtocol = HelloHelper.CreateProtocol(helloCommand, _serverApi);
            var helloResult = HelloHelper.GetResult(connection, helloProtocol, cancellationToken);
            if (connection.Settings.LoadBalanced && !helloResult.ServiceId.HasValue)
            {
                throw new InvalidOperationException("Driver attempted to initialize in load balancing mode, but the server does not support this mode.");
            }

            return new (new ConnectionDescription(connection.ConnectionId, helloResult), authenticators);
        }

        public async Task<ConnectionInitializerContext> SendHelloAsync(IConnection connection, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(connection, nameof(connection));
            var authenticators = GetAuthenticators(connection.EndPoint, connection.Settings);
            var helloCommand = CreateInitialHelloCommand(authenticators, connection.Settings.LoadBalanced, cancellationToken);
            var helloProtocol = HelloHelper.CreateProtocol(helloCommand, _serverApi);
            var helloResult = await HelloHelper.GetResultAsync(connection, helloProtocol, cancellationToken).ConfigureAwait(false);
            if (connection.Settings.LoadBalanced && !helloResult.ServiceId.HasValue)
            {
                throw new InvalidOperationException("Driver attempted to initialize in load balancing mode, but the server does not support this mode.");
            }

            return new (new ConnectionDescription(connection.ConnectionId, helloResult), authenticators);
        }

        // private methods
        private CommandWireProtocol<BsonDocument> CreateGetLastErrorProtocol(ServerApi serverApi)
        {
            var getLastErrorCommand = new BsonDocument("getLastError", 1);
            var getLastErrorProtocol = new CommandWireProtocol<BsonDocument>(
                databaseNamespace: DatabaseNamespace.Admin,
                command: getLastErrorCommand,
                secondaryOk: true,
                resultSerializer: BsonDocumentSerializer.Instance,
                messageEncoderSettings: null,
                serverApi: serverApi);
            return getLastErrorProtocol;
        }

        private BsonDocument CreateInitialHelloCommand(IReadOnlyList<IAuthenticator> authenticators, bool loadBalanced = false, CancellationToken cancellationToken = default)
        {
            var command = HelloHelper.CreateCommand(_serverApi, loadBalanced: loadBalanced);
            HelloHelper.AddClientDocumentToCommand(command, _clientDocument);
            HelloHelper.AddCompressorsToCommand(command, _compressors);
            return HelloHelper.CustomizeCommand(command, authenticators, cancellationToken);
        }

        private List<IAuthenticator> GetAuthenticators(EndPoint endpoint, ConnectionSettings settings) => settings.AuthenticatorFactories.Select(f => f.Create(endpoint)).ToList();

        private ConnectionDescription UpdateConnectionIdWithServerValue(ConnectionDescription description, BsonDocument getLastErrorResult)
        {
            if (getLastErrorResult.TryGetValue("connectionId", out var connectionIdBsonValue))
            {
                description = UpdateConnectionIdWithServerValue(description, connectionIdBsonValue.ToInt32());
            }

            return description;
        }

        private ConnectionDescription UpdateConnectionIdWithServerValue(ConnectionDescription description, int serverValue)
        {
            var connectionId = description.ConnectionId.WithServerValue(serverValue);
            description = description.WithConnectionId(connectionId);

            return description;
        }
    }
}
