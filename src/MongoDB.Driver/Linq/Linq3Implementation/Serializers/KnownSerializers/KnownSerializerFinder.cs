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

using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using ExpressionVisitor = System.Linq.Expressions.ExpressionVisitor;

namespace MongoDB.Driver.Linq.Linq3Implementation.Serializers.KnownSerializers
{
    internal class KnownSerializerFinder<T> : ExpressionVisitor
    {
        private readonly BsonClassMapSerializer<T> _providerCollectionDocumentSerializer;

        #region static
        // public static methods
        public static KnownSerializersRegistry FindKnownSerializers(Expression root, BsonClassMapSerializer<T> providerCollectionDocumentSerializer)
        {
            var visitor = new KnownSerializerFinder<T>(providerCollectionDocumentSerializer);
            visitor.Visit(root);
            return visitor._registry;
        }
        #endregion

        // private fields
        private KnownSerializersNode _expressionKnownSerializers = null;
        private readonly KnownSerializersRegistry _registry = new KnownSerializersRegistry();

        // constructors
        private KnownSerializerFinder(BsonClassMapSerializer<T> providerCollectionDocumentSerializer)
        {
            _providerCollectionDocumentSerializer = providerCollectionDocumentSerializer;
        }

        // public methods
        public override Expression Visit(Expression node)
        {
            if (node == null) return null;

            _expressionKnownSerializers = new KnownSerializersNode(_expressionKnownSerializers);
            _expressionKnownSerializers.AddKnownSerializer(_providerCollectionDocumentSerializer.ValueType, _providerCollectionDocumentSerializer);
            if (node is MemberExpression memberExpression)
            {
                if (_providerCollectionDocumentSerializer.TryGetMemberSerializationInfo(memberExpression.Member.Name, out var memberSerializer))
                {
                    _expressionKnownSerializers.AddKnownSerializer(memberExpression.Type, memberSerializer.Serializer);
                }
            }
            _registry.Add(node, _expressionKnownSerializers);

            var result = base.Visit(node);

            _expressionKnownSerializers = _expressionKnownSerializers.Parent;
            return result;
        }
    }
}
