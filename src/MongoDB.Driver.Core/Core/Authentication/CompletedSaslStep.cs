using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// Represents a completed SASL step.
    /// </summary>
    internal sealed class CompletedSaslStep : ISaslStep
    {
        // fields
        private readonly byte[] _bytesToSendToServer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedSaslStep"/> class.
        /// </summary>
        public CompletedSaslStep()
            : this(Array.Empty<byte>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletedSaslStep"/> class.
        /// </summary>
        /// <param name="bytesToSendToServer">The bytes to send to server.</param>
        public CompletedSaslStep(byte[] bytesToSendToServer)
        {
            _bytesToSendToServer = bytesToSendToServer;
        }

        // properties
        /// <inheritdoc/>
        public byte[] BytesToSendToServer
        {
            get { return _bytesToSendToServer; }
        }

        /// <inheritdoc/>
        public bool IsComplete
        {
            get { return true; }
        }

        /// <inheritdoc/>
        public ISaslStep Transition(SaslConversation conversation, byte[] bytesReceivedFromServer)
        {
            if (bytesReceivedFromServer?.Length > 0)
            {
                // should not be reached
                throw new InvalidOperationException("Not all authentication response has been handled.");
            }

            throw new InvalidOperationException("Sasl conversation has completed.");
        }

        public Task<ISaslStep> TransitionAsync(
            SaslConversation conversation,
            byte[] bytesReceivedFromServer,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Transition(conversation, bytesReceivedFromServer));
    }
}
