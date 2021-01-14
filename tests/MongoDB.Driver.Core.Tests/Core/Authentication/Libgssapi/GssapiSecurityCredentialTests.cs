using System.Security;
using FluentAssertions;
using MongoDB.Driver.Core.Authentication.Libgssapi;
using Xunit;

namespace MongoDB.Driver.Core.Tests.Core.Authentication.Libgssapi
{
    public class GssapiSecurityCredentialTests
    {
        [Fact]
        public void Should_acquire_gssapi_security_credential_with_username_and_password()
        {
            const string username = "drivers@LDAPTEST.10GEN.CC";
            const string password = "powerbook17";
            var securePassword = new SecureString();
            foreach (var c in password)
            {
                securePassword.AppendChar(c);
            }

            var credential = GssapiSecurityCredential.Acquire(username, securePassword);
            credential.Should().NotBeNull();
        }
    }
}
