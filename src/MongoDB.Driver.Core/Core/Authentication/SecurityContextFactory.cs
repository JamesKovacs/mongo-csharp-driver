using System.Runtime.InteropServices;
using System.Security;
using MongoDB.Driver.Core.Authentication.Libgssapi;
using MongoDB.Driver.Core.Authentication.Sspi;

namespace MongoDB.Driver.Core.Authentication
{
    internal static class SecurityContextFactory
    {
        public static ISecurityContext InitializeSecurityContext(SaslAuthenticator.SaslConversation conversation, string servicePrincipalName, string authorizationId, SecureString password)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SecurityCredential credential;
                try
                {
                    credential = SecurityCredential.Acquire(SspiPackage.Kerberos, authorizationId, password);
                    conversation.RegisterItemForDisposal(credential);
                }
                catch (Win32Exception ex)
                {
                    throw new MongoAuthenticationException(conversation.ConnectionId, "Unable to acquire security credential.", ex);
                }

                return new SspiSecurityContext(servicePrincipalName, credential);
            }
            else
            {
                return new GssapiSecurityContext();
            }
        }
    }
}
