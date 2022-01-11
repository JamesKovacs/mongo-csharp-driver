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
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.WireProtocol;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// Represents an explain operation.
    /// </summary>
    internal class ExplainOperation<TResult> : IReadOperation<CursorBatch<TResult>>, IExecutableInRetryableReadContext<CursorBatch<TResult>>
    {
        // fields
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly BsonDocument _command;
        private readonly MessageEncoderSettings _messageEncoderSettings;
        private readonly IBsonSerializer<TResult> _resultSerializer;
        private bool _retryRequested;
        private ExplainVerbosity _verbosity;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExplainOperation{TResult}"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="command">The command.</param>
        /// <param name="resultSerializer">The result serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        public ExplainOperation(DatabaseNamespace databaseNamespace, BsonDocument command, IBsonSerializer<TResult> resultSerializer, MessageEncoderSettings messageEncoderSettings)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _command = Ensure.IsNotNull(command, nameof(command));
            _messageEncoderSettings = Ensure.IsNotNull(messageEncoderSettings, nameof(messageEncoderSettings));
            _resultSerializer = Ensure.IsNotNull(resultSerializer, nameof(resultSerializer));
            _verbosity = ExplainVerbosity.AllPlansExecution; // default
        }

        // properties
        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
        }

        /// <summary>
        /// Gets the command to be explained.
        /// </summary>
        /// <value>
        /// The command to be explained.
        /// </value>
        public BsonDocument Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets the message encoder settings.
        /// </summary>
        /// <value>
        /// The message encoder settings.
        /// </value>
        public MessageEncoderSettings MessageEncoderSettings
        {
            get { return _messageEncoderSettings; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to retry.
        /// </summary>
        /// <value>Whether to retry.</value>
        public bool RetryRequested
        {
            get { return _retryRequested; }
            set { _retryRequested = value; }
        }

        /// <summary>
        /// Gets or sets the verbosity.
        /// </summary>
        /// <value>
        /// The verbosity.
        /// </value>
        public ExplainVerbosity Verbosity
        {
            get { return _verbosity; }
            set { _verbosity = value; }
        }

        // public methods
        /// <inheritdoc/>
        public CursorBatch<TResult> Execute(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var context = RetryableReadContext.Create(binding, _retryRequested, cancellationToken))
            {
                return Execute(context, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task<CursorBatch<TResult>> ExecuteAsync(IReadBinding binding, CancellationToken cancellationToken)
        {
            Ensure.IsNotNull(binding, nameof(binding));

            using (var context = await RetryableReadContext.CreateAsync(binding, _retryRequested, cancellationToken).ConfigureAwait(false))
            {
                return await ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public CursorBatch<TResult> Execute(RetryableReadContext context, CancellationToken cancellationToken)
        {
            var operation = CreateReadOperation();
            var result = operation.Execute(context, cancellationToken);
            return TransformToCursorBatch(result);
        }

        /// <inheritdoc/>
        public async Task<CursorBatch<TResult>> ExecuteAsync(RetryableReadContext context, CancellationToken cancellationToken)
        {
            var operation = CreateReadOperation();
            var result = await operation.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            return TransformToCursorBatch(result);
        }

        // private methods
        private static string ConvertVerbosityToString(ExplainVerbosity verbosity)
        {
            switch (verbosity)
            {
                case ExplainVerbosity.AllPlansExecution:
                    return "allPlansExecution";
                case ExplainVerbosity.ExecutionStats:
                    return "executionStats";
                case ExplainVerbosity.QueryPlanner:
                    return "queryPlanner";
                default:
                    var message = string.Format("Unsupported explain verbosity: {0}.", verbosity.ToString());
                    throw new InvalidOperationException(message);
            }
        }

        internal BsonDocument CreateCommand()
        {
            return new BsonDocument
            {
                { "explain", _command },
                { "verbosity", ConvertVerbosityToString(_verbosity) }
            };
        }

        private ReadCommandOperation<BsonDocument> CreateReadOperation()
        {
            var command = CreateCommand();
            return new ReadCommandOperation<BsonDocument>(
                _databaseNamespace,
                command,
                BsonDocumentSerializer.Instance,
                _messageEncoderSettings)
            {
                RetryRequested = _retryRequested
            };
        }

        private CursorBatch<TResult> TransformToCursorBatch(BsonDocument explainedResult)
        {
            using var bsonDocumentReader = new BsonDocumentReader(explainedResult);
            var context = BsonDeserializationContext.CreateRoot(bsonDocumentReader);
            var result = _resultSerializer.Deserialize(context);

            return new CursorBatch<TResult>(
                cursorId: 0,
                documents: new[] { result });
        }
    }
}
