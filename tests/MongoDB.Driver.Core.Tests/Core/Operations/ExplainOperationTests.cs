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
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.TestHelpers.XunitExtensions;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Core.Operations
{
    public class ExplainOperationTests : OperationTestBase
    {
        private readonly BsonDocument _command;

        public ExplainOperationTests()
        {
            _command = new BsonDocument
            {
                { "count", _collectionNamespace.CollectionName }
            };
        }

        [Fact]
        public void Constructor_should_throw_when_collection_namespace_is_null()
        {
            var ex = Record.Exception(() => new ExplainOperation<BsonDocument>(null, _command, BsonDocumentSerializer.Instance, _messageEncoderSettings));

            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_command_is_null()
        {
            var ex = Record.Exception(() => new ExplainOperation<BsonDocument>(_databaseNamespace, null, BsonDocumentSerializer.Instance, _messageEncoderSettings));

            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_result_serializer_is_null()
        {
            var ex = Record.Exception(() => new ExplainOperation<BsonDocument>(_databaseNamespace, _command, null, _messageEncoderSettings));

            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_throw_when_message_encoder_settings_is_null()
        {
            var ex = Record.Exception(() => new ExplainOperation<BsonDocument>(_databaseNamespace, _command, BsonDocumentSerializer.Instance, null));

            ex.Should().BeOfType<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_should_initialize_subject()
        {
            var subject = new ExplainOperation<BsonDocument>(_databaseNamespace, _command, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                RetryRequested = true
            };

            subject.DatabaseNamespace.Should().Be(_databaseNamespace);
            subject.Command.Should().Be(_command);
            subject.MessageEncoderSettings.Should().BeEquivalentTo(_messageEncoderSettings);
            subject.RetryRequested.Should().Be(true);
            subject.Verbosity.Should().Be(ExplainVerbosity.AllPlansExecution);
        }

        [Theory]
        [InlineData(ExplainVerbosity.AllPlansExecution, "allPlansExecution")]
        [InlineData(ExplainVerbosity.ExecutionStats, "executionStats")]
        [InlineData(ExplainVerbosity.QueryPlanner, "queryPlanner")]
        public void CreateCommand_should_return_expected_result(ExplainVerbosity verbosity, string verbosityString)
        {
            var subject = new ExplainOperation<BsonDocument>(_databaseNamespace, _command, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                Verbosity = verbosity
            };

            var expectedResult = new BsonDocument
            {
                { "explain", _command },
                { "verbosity", verbosityString }
            };

            var result = subject.CreateCommand();

            result.Should().Be(expectedResult);
        }

        [SkippableTheory]
        [ParameterAttributeData]
        public void Execute_should_not_throw_when_collection_does_not_exist(
            [Values(false, true)]
            bool async)
        {
            RequireServer.Check();
            DropCollection();
            var subject = new ExplainOperation<BsonDocument>(_databaseNamespace, _command, BsonDocumentSerializer.Instance, _messageEncoderSettings);

            var result = ExecuteOperation(subject, async);

            result.Should().NotBeNull();
        }

        [SkippableTheory]
        [ParameterAttributeData]
        public void Execute_should_decorate_response(
            [Values(ExplainVerbosity.AllPlansExecution, ExplainVerbosity.ExecutionStats, ExplainVerbosity.QueryPlanner)] ExplainVerbosity explainVerbosity,
            [Values(false, true)] bool async)
        {
            RequireServer.Check();
            EnsureCollectionExists();
            var subject = new ExplainOperation<BsonDocument>(_databaseNamespace, _command, BsonDocumentSerializer.Instance, _messageEncoderSettings)
            {
                Verbosity = explainVerbosity
            };

            var result = ExecuteOperation(subject, async);

            result.AtClusterTime.Should().BeNull();
            result.PostBatchResumeToken.Should().BeNull();
            result.CollectionNamespace.Should().BeNull();
            result.CursorId.Should().Be(0);
            result.Documents.Should().HaveCount(1);
            var explainDocument = result.Documents.First();
            explainDocument.Elements
                .Should()
                .ContainSingle(e => e.Name == "serverInfo"); // this node presents in $explain response regardless server version
        }

        // private method
        private void EnsureCollectionExists()
        {
            Insert(BsonDocument.Parse("{ x : 1 }"));
        }
    }
}
