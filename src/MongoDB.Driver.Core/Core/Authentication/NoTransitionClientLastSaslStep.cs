using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// Represents a last SASL step.
    /// </summary>
    internal sealed class NoTransitionClientLastSaslStep : SaslAuthenticator.ISaslStep
    {
        private readonly byte[] _bytesToSendToServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoTransitionClientLastSaslStep"/> class.
        /// </summary>
        public NoTransitionClientLastSaslStep(byte[] bytesToSendToServer)
        {
            _bytesToSendToServer = bytesToSendToServer;
        }

        /// <inheritdoc/>
        public byte[] BytesToSendToServer => _bytesToSendToServer;

        /// <inheritdoc/>
        public bool IsComplete => false;

        /// <inheritdoc/>
        public SaslAuthenticator.ISaslStep Transition(SaslAuthenticator.SaslConversation conversation, byte[] bytesReceivedFromServer)
        {
            if (bytesReceivedFromServer?.Length > 0)
            {
                // should not be reached
                throw new InvalidOperationException("Not all authentication response has been handled.");
            }

            return new SaslAuthenticator.CompletedStep();
        }

        public Task<SaslAuthenticator.ISaslStep> TransitionAsync(SaslAuthenticator.SaslConversation conversation, byte[] bytesReceivedFromServer, CancellationToken cancellationToken)
            => Task.FromResult(Transition(conversation, bytesReceivedFromServer));
    }
}
