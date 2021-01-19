using System;
using FluentAssertions;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.Authentication.Libgssapi;
using MongoDB.Shared;
using Xunit;

namespace MongoDB.Driver.Core.Tests.Core.Authentication.Libgssapi
{
    [Trait("Category", "Authentication")]
    [Trait("Category", "GssapiMechanism")]
    public class GssapiSecurityCredentialTests
    {
        private string _username;
        private string _password;

        public GssapiSecurityCredentialTests()
        {
            var authGssapi = GetEnvironmentVariable("AUTH_GSSAPI");
            var authParts = authGssapi.Split(":");
            _username = authParts[0];
            _password = authParts[1];
        }

        [SkippableFact]
        public void Should_acquire_gssapi_security_credential_with_username_and_password()
        {
            RequireEnvironment.Check().EnvironmentVariable("GSSAPI_TESTS_ENABLED");

            var securePassword = SecureStringHelper.ToSecureString(_password);
            var credential = GssapiSecurityCredential.Acquire(_username, securePassword);
            credential.Should().NotBeNull();
        }

        [SkippableFact]
        public void Should_acquire_gssapi_security_credential_with_username_only()
        {
            RequireEnvironment.Check().EnvironmentVariable("GSSAPI_TESTS_ENABLED");

            var credential = GssapiSecurityCredential.Acquire(_username);
            credential.Should().NotBeNull();
        }

        [SkippableFact]
        public void Should_fail_to_acquire_gssapi_security_credential_with_username_and_bad_password()
        {
            RequireEnvironment.Check().EnvironmentVariable("GSSAPI_TESTS_ENABLED");

            var securePassword = SecureStringHelper.ToSecureString("BADPASSWORD");

            var exception = Record.Exception(() => GssapiSecurityCredential.Acquire(_username, securePassword));
            exception.Should().BeOfType<LibgssapiException>();
        }

        private string GetEnvironmentVariable(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} has not been configured.");
    }
}
