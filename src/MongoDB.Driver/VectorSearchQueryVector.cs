﻿/* Copyright 2010-present MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Vector search query vector.
    /// </summary>
    public sealed class VectorSearchQueryVector
    {
        /// <summary>
        /// Gets the underlying BSON array.
        /// </summary>
        public BsonArray Array { get; }

        private VectorSearchQueryVector(BsonArray array)
        {
            Ensure.IsNotNullOrEmpty(array, nameof(array));
            Array = array;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:double[]"/> to <see cref="VectorSearchQueryVector"/>.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VectorSearchQueryVector(double[] array) =>
            new(new QueryVectorBsonArray<double>(array));

        /// <summary>
        /// Performs an implicit conversion from a of <see cref="ReadOnlyMemory{T}"/> to <see cref="VectorSearchQueryVector"/>.
        /// </summary>
        /// <param name="readOnlyMemory">The readOnlyMemory.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VectorSearchQueryVector(ReadOnlyMemory<double> readOnlyMemory) =>
            new(new QueryVectorBsonArray<double>(readOnlyMemory));

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:float[]"/> to <see cref="VectorSearchQueryVector"/>.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VectorSearchQueryVector(float[] array) =>
            new(new QueryVectorBsonArray<float>(array));

        /// <summary>
        /// Performs an implicit conversion from a of <see cref="ReadOnlyMemory{T}"/> to <see cref="VectorSearchQueryVector"/>.
        /// </summary>
        /// <param name="readOnlyMemory">The readOnlyMemory.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VectorSearchQueryVector(ReadOnlyMemory<float> readOnlyMemory) =>
            new(new QueryVectorBsonArray<float>(readOnlyMemory));

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:int[]"/> to <see cref="VectorSearchQueryVector"/>.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VectorSearchQueryVector(int[] array) =>
            new(new QueryVectorBsonArray<int>(array));

        /// <summary>
        /// Performs an implicit conversion from a of <see cref="ReadOnlyMemory{T}"/> to <see cref="VectorSearchQueryVector"/>.
        /// </summary>
        /// <param name="readOnlyMemory">The readOnlyMemory.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator VectorSearchQueryVector(ReadOnlyMemory<int> readOnlyMemory) =>
            new(new QueryVectorBsonArray<int>(readOnlyMemory));
    }

    [BsonSerializer(typeof(QueryVectorArraySerializer<>))]
    internal sealed class QueryVectorBsonArray<T> : BsonArray
        where T : struct, IConvertible
    {
        private readonly ReadOnlyMemory<T> _memory;

        public QueryVectorBsonArray(ReadOnlyMemory<T> memory)
        {
            _memory = memory;
        }

        public QueryVectorBsonArray(T[] array)
        {
            _memory = new ReadOnlyMemory<T>(array);
        }

        public override int Count => _memory.Length;

        public ReadOnlySpan<T> Span => _memory.Span;

        public override IEnumerable<BsonValue> Values
        {
            get
            {
                for (int i = 0; i < _memory.Length; i++)
                {
                    yield return _memory.Span[i].ToDouble(null);
                }
            }
        }
    }

    internal sealed class QueryVectorArraySerializer<T> : BsonValueSerializerBase<QueryVectorBsonArray<T>>
        where T : struct, IConvertible
    {
        // constructors
        public QueryVectorArraySerializer()
            : base(BsonType.Array)
        {
        }

        // protected methods
        protected override QueryVectorBsonArray<T> DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args) =>
            throw new NotImplementedException();

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, QueryVectorBsonArray<T> value)
        {
            var bsonWriter = context.Writer;
            var span = value.Span;

            bsonWriter.WriteStartArray();

            for (int i = 0; i < value.Count; i++)
            {
                bsonWriter.WriteDouble(span[i].ToDouble(null));
            }

            bsonWriter.WriteEndArray();
        }
    }
}
