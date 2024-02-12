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

using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using MongoDB.Driver.Core.Authentication.Oidc;
using Moq;
using Xunit;

namespace MongoDB.Driver.Core.Tests.Core.Authentication.Oidc
{
    public class OidcConfigurationTests
    {
        private static readonly IOidcCredentialsProviders __buildInProvidersMock;
        private static readonly IOidcCallbackProvider __callbackMock = new Mock<IOidcCallbackProvider>().Object;
        private static readonly EndPoint __endPoint = new DnsEndPoint("localhost", 27017);

        static OidcConfigurationTests()
        {
            var buildInProvidersMock = new Mock<IOidcCredentialsProviders>();
            buildInProvidersMock.Setup(p => p.Aws)
                .Returns(__callbackMock);

            __buildInProvidersMock = buildInProvidersMock.Object;
        }

        [Theory]
        [MemberData(nameof(ValidConfigurationTestCases))]
        public void Should_accept_valid_arguments(string principalName, IReadOnlyDictionary<string, object> mechanismProperties)
        {
            var configuration = new OidcConfiguration(__endPoint, principalName, mechanismProperties, __buildInProvidersMock);

            configuration.PrincipalName.Should().Be(principalName);
            configuration.Callback.Should().NotBeNull();
        }

        public static IEnumerable<object[]> ValidConfigurationTestCases = new[]
        {
            new object[] { null, new Dictionary<string, object> { ["PROVIDER_NAME"] = "aws" } },
            new object[] { "name", new Dictionary<string, object> { ["PROVIDER_NAME"] = "aws" } },
            new object[] { "name", new Dictionary<string, object> { ["OIDC_CALLBACK"] = __callbackMock } },
        };

        [Theory]
        [MemberData(nameof(InvalidConfigurationTestCases))]
        public void Should_throw_on_invalid_arguments(string principalName, IReadOnlyDictionary<string, object> mechanismProperties)
        {
            var exception = Record.Exception(() =>
                new OidcConfiguration(__endPoint, principalName, mechanismProperties, __buildInProvidersMock));

            exception.Should().NotBeNull();
        }

        public static IEnumerable<object[]> InvalidConfigurationTestCases = new[]
        {
            new object[] { null, null },
            new object[] { null, new Dictionary<string, object>() },
            new object[] { "name", null },
            new object[] { "name", new Dictionary<string, object>() },
            new object[] { "name", new Dictionary<string, object> { ["unknown_property"] = 42 } },
            new object[] { "name", new Dictionary<string, object> { ["PROVIDER_NAME"] = null } },
            new object[] { "name", new Dictionary<string, object> { ["PROVIDER_NAME"] = "" } },
            new object[] { "name", new Dictionary<string, object> { ["PROVIDER_NAME"] = 1 } },
            new object[] { "name", new Dictionary<string, object> { ["PROVIDER_NAME"] = "unknown provider" } },
            new object[] { "name", new Dictionary<string, object> { ["OIDC_CALLBACK"] = null } },
            new object[] { "name", new Dictionary<string, object> { ["OIDC_CALLBACK"] = "invalid type" } },
            new object[] { "name", new Dictionary<string, object> { ["OIDC_CALLBACK"] = __callbackMock, ["PROVIDER_NAME"] = "aws" } },
        };
    }
}
