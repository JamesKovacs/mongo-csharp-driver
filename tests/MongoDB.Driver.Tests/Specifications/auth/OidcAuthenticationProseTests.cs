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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Authentication.External;
using MongoDB.Driver.Core.Authentication.Oidc;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers;
using MongoDB.Driver.Core.TestHelpers.Authentication;
using MongoDB.Driver.Core.TestHelpers.Logging;
using MongoDB.Driver.TestHelpers;
using MongoDB.TestHelpers.XunitExtensions;
using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace MongoDB.Driver.Tests.Communication.Security
{
    [Trait("Category", "Authentication")]
    [Trait("Category", "OidcMechanism")]
    public class OidcAuthenticationProseTests : LoggableTestClass
    {
        // some auth configuration may support only this name
        private const string DatabaseName = "test";
        private const string CollectionName = "collName";

        private const string OidcTokensDirEnvName = "OIDC_TOKEN_DIR";

        private const string SecondaryPreferedConnectionStringSuffix = "&readPreference=secondaryPreferred";
        private const string DirectConnectionStringSuffix = "&directConnection=true";
        private const string DirectConnectionSecondaryPreferedConnectionStringSuffix = SecondaryPreferedConnectionStringSuffix + DirectConnectionStringSuffix;
        private const string DefaultTokenName = "test_user1";

        public OidcAuthenticationProseTests(ITestOutputHelper output) : base(output)
        {
            //OidcTestHelper.ClearStaticCache(ExternalCredentialsAuthenticators.Instance);
            EnsureOidcIsConfigured();
            OidcCallbackAdapterCachingFactory.Instance.Reset();
        }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L49
        [Theory]
        [ParameterAttributeData]
        public async Task Callback_authentication_callback_called_during_authentication([Values(false, true)]bool async)
        {
            var callbackProviderMock = CreateOidcCallback(GetAccessTokenValue("test_user1_expires"));
            var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var exception = async
                ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
                : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));

            exception.Should().BeNull();
            if (async)
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Once());
            }
            else
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Once());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
            }
        }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L57
        [Theory]
        [ParameterAttributeData]
        public async Task Callback_authentication_callback_called_once_for_multiple_connections([Values(false, true)]bool async)
        {
            var callbackProviderMock = CreateOidcCallback(GetAccessTokenValue("test_user1"));
            var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                for (var i = 0; i < 100; i++)
                {
                    if (async)
                    {
                        await collection.FindAsync(Builders<BsonDocument>.Filter.Empty);
                    }
                    else
                    {
                        collection.FindSync(Builders<BsonDocument>.Filter.Empty);
                    }
                }
            }));

            await Task.WhenAll(tasks);

            if (async)
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Once());
            }
            else
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Once());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
            }
        }

        //https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L69
        [Theory]
        [ParameterAttributeData]
        public async Task Callback_validation_valid_callback_inputs([Values(false, true)] bool async)
        {
            var callbackProviderMock = CreateOidcCallback(GetAccessTokenValue("test_user1"));
            var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var exception = async
                ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
                : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));

            exception.Should().BeNull();
            if (async)
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.Is<OidcCallbackParameters>(p => p.Version == 1), It.IsAny<CancellationToken>()), Times.Once());
            }
            else
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.Is<OidcCallbackParameters>(p => p.Version == 1), It.IsAny<CancellationToken>()), Times.Once());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
            }
        }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L78
        [Theory]
        [ParameterAttributeData]
        public async Task Callback_validation_callback_returns_null([Values(false, true)] bool async)
        {
            var callbackProviderMock = new Mock<IOidcCallbackProvider>();
            var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var exception = async
                ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
                : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));

            exception.Should().BeOfType<MongoConnectionException>();
        }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L85
        [Theory]
        [ParameterAttributeData]
        public async Task Callback_validation_callback_returns_missing_data([Values(false, true)] bool async)
        {
            var callbackProviderMock = CreateOidcCallback("wrong token");
            var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var exception = async
                ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
                : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));

            exception.Should().BeOfType<MongoConnectionException>();
        }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L92
        [Theory]
        [ParameterAttributeData]
        public async Task Callback_validation_invalid_client_configuration([Values(false, true)] bool async)
        {
            var callbackProviderMock = CreateOidcCallback(GetAccessTokenValue("test_user1"));
            var credential = MongoCredential.CreateOidcCredential(callbackProviderMock.Object)
                .WithMechanismProperty("PROVIDER_NAME", "aws");
            var clientSettings = CreateOidcMongoClientSettings(credential);
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var exception = async
                ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
                : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));

            exception.Should().BeOfType<MongoConnectionException>();
        }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L101C7-L101C81
        // [Theory]
        // [ParameterAttributeData]
        // public async Task Authentication_failure_with_cached_tokens_fetch_new_and_retry([Values(false, true)] bool async)
        // {
        //     var callbackProviderMock = CreateOidcCallback("wrong token", GetAccessTokenValue("test_user1"));
        //     var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
        //     var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);
        //
        //     var db = client.GetDatabase(DatabaseName);
        //     var collection = db.GetCollection<BsonDocument>(CollectionName);
        //
        //     var exception = async
        //         ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
        //         : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));
        //
        //     exception.Should().BeNull();
        //     if (async)
        //     {
        //         callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
        //         // Check for 2 calls because first call will return poisoned response that cause failure of initial auth, clear the cache and second call to get proper token
        //         callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        //     }
        //     else
        //     {
        //         // Check for 2 calls because first call will return poisoned response that cause failure of initial auth, clear the cache and second call to get proper token
        //         callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        //         callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
        //     }
        // }

        // https://github.com/mongodb/specifications/blob/3b8e7e4135f2615fedef1ea6cd2046f5be5a1725/source/auth/tests/mongodb-oidc.rst?plain=1#L110C31-L110C68
        [Theory]
        [ParameterAttributeData]
        public async Task Authentication_failure_without_cached_tokens_return_error([Values(false, true)] bool async)
        {
            var callbackProviderMock = CreateOidcCallback("wrong token");
            var clientSettings = CreateOidcMongoClientSettings(MongoCredential.CreateOidcCredential(callbackProviderMock.Object));
            var client = DriverTestConfiguration.CreateDisposableClient(clientSettings);

            var db = client.GetDatabase(DatabaseName);
            var collection = db.GetCollection<BsonDocument>(CollectionName);

            var exception = async
                ? await Record.ExceptionAsync(() => collection.FindAsync(Builders<BsonDocument>.Filter.Empty))
                : Record.Exception(() => collection.FindSync(Builders<BsonDocument>.Filter.Empty));

            exception.Should().BeOfType<MongoConnectionException>();
            if (async)
            {
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
                // Check for 2 calls because first call will return poisoned response that cause failure of initial auth, clear the cache and second call to get proper token
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Once());
            }
            else
            {
                // Check for 2 calls because first call will return poisoned response that cause failure of initial auth, clear the cache and second call to get proper token
                callbackProviderMock.Verify(x => x.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Once());
                callbackProviderMock.Verify(x => x.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()), Times.Never());
            }
        }


        private void EnsureOidcIsConfigured() =>
            // EG also requires aws_test_secrets_role
            RequireEnvironment
                .Check()
                .EnvironmentVariable(OidcTokensDirEnvName)
                .EnvironmentVariable("OIDC_TESTS_ENABLED");

        private MongoClientSettings CreateOidcMongoClientSettings(MongoCredential credential, string applicationName = null, EventCapturer eventCapturer = null)
        {
            var settings = DriverTestConfiguration.GetClientSettings();
            settings.ApplicationName = applicationName;
            settings.RetryReads = false;
            settings.RetryWrites = false;
            settings.MinConnectionPoolSize = 0;
            settings.Credential = credential;
            if (eventCapturer != null)
            {
                settings.ClusterConfigurator = (builder) => builder.Subscribe(eventCapturer);
            }

            return settings;
        }

        private Mock<IOidcCallbackProvider> CreateOidcCallback(string accessToken)
        {
            var callbackProvider = new Mock<IOidcCallbackProvider>();
            var response = new OidcCallbackResponse(accessToken, null);
            callbackProvider
                .Setup(c => c.GetResponse(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()))
                .Returns(response);
            callbackProvider
                .Setup(c => c.GetResponseAsync(It.IsAny<OidcCallbackParameters>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(response));

            return callbackProvider;
        }

        private string GetAccessTokenValue(string tokenName = DefaultTokenName)
        {
            var tokenPath = Path.Combine(Environment.GetEnvironmentVariable(OidcTokensDirEnvName), tokenName);
            Ensure.That(File.Exists(tokenPath), $"OIDC token {tokenPath} doesn't exist.");

            return File.ReadAllText(tokenPath);
        }
    }
}
