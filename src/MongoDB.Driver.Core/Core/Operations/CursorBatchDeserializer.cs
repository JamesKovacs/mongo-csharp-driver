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
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.WireProtocol;

namespace MongoDB.Driver.Core.Operations
{
    internal class CursorBatchDeserializer<TResult> : SerializerBase<CursorBatch<TResult>>
    {
        private readonly IBsonSerializer<TResult> _resultSerializer;

        public CursorBatchDeserializer(IBsonSerializer<TResult> resultSerializer)
        {
            _resultSerializer = resultSerializer;
        }

        public override CursorBatch<TResult> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;
            CursorBatch<TResult> result = default;
            reader.ReadStartDocument();
            while (reader.ReadBsonType() != 0)
            {
                var elementName = reader.ReadName();
                switch (elementName)
                {
                    case "cursor":
                        {
                            var cursorDeserializer = new AsyncCursorDeserializer(_resultSerializer);
                            result = cursorDeserializer.Deserialize(context);
                            break;
                        }

                    default:
                        reader.SkipValue();
                        break;
                }
            }
            reader.ReadEndDocument();
            return result;
        }

        private class AsyncCursorDeserializer : SerializerBase<CursorBatch<TResult>>
        {
            private readonly IBsonSerializer<TResult> _resultSerializer;

            public AsyncCursorDeserializer(IBsonSerializer<TResult> resultSerializer)
            {
                _resultSerializer = resultSerializer;
            }

            public override CursorBatch<TResult> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                var reader = context.Reader;
                reader.ReadStartDocument();

                BsonTimestamp atClusterTime = null;
                long? cursorId = null;
                CollectionNamespace collectionNamespace = null;
                TResult[] documents = null;
                BsonDocument postBatchResumeToken = null;
                while (reader.ReadBsonType() != 0)
                {
                    var elementName = reader.ReadName();
                    switch (elementName)
                    {
                        case "atClusterTime":
                            atClusterTime = BsonTimestampSerializer.Instance.Deserialize(context);
                            break;

                        case "id":
                            cursorId = new Int64Serializer().Deserialize(context);
                            break;

                        case "ns":
                            var ns = reader.ReadString();
                            collectionNamespace = CollectionNamespace.FromFullName(ns);
                            break;

                        case "firstBatch": // values are mutually exclusive
                        case "nextBatch":
                            if (documents != null)
                            {
                                throw new InvalidOperationException("The BatchDocuments value has already been initialized."); // should not be reached
                            }
                            var arraySerializer = new ArraySerializer<TResult>(_resultSerializer);
                            documents = arraySerializer.Deserialize(context);
                            break;

                        case "postBatchResumeToken":
                            postBatchResumeToken = BsonDocumentSerializer.Instance.Deserialize(context);
                            break;

                        default:
                            reader.SkipValue();
                            break;
                    }
                }
                reader.ReadEndDocument();

                return new CursorBatch<TResult>(
                    cursorId.GetValueOrDefault(0),
                    documents,
                    postBatchResumeToken,
                    atClusterTime,
                    collectionNamespace);
            }
        }
    }
}
