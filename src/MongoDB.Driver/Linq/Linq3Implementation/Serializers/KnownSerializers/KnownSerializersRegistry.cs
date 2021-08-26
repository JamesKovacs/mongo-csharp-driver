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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers
{
    internal class KnownSerializersRegistry
    {
        // private fields
        private readonly Dictionary<Expression, KnownSerializersNode> _registry = new Dictionary<Expression, KnownSerializersNode>();

        // public methods
        public void Add(Expression expression, KnownSerializersNode knownSerializers)
        {
            if (_registry.ContainsKey(expression)) return;

            _registry.Add(expression, knownSerializers);
        }

        public HashSet<IBsonSerializer> GetPossibleSerializers(Expression expression, Type type)
        {
            if (_registry.TryGetValue(expression, out var knownSerializers))
            {
                return knownSerializers.GetPossibleSerializers(type);
            }
            else
            {
                return new HashSet<IBsonSerializer>();
            }
        }

        public IBsonSerializer GetSerializer(Expression expr)
        {
            return _registry[expr].KnownSerializers.First().Value.First();
            // return BsonSerializer.LookupSerializer(expr.Type);
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            if (type == typeof(DateTime))
                return new DateTimeSerializer();
            if (type == typeof(bool))
                return new BooleanSerializer();
            if (type == typeof(double))
                return new DoubleSerializer();
            return BsonSerializer.LookupSerializer(type);
        }
    }
}
