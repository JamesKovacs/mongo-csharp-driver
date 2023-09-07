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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Linq3Implementation.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// A definition of a single field to be set.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class SetFieldDefinition<TDocument>
    {
        /// <summary>
        /// Renders the SetFieldDefinition.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <param name="linqProvider">The linq provider.</param>
        /// <returns>The rendered SetFieldDefinition.</returns>
        public abstract BsonElement Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider);
    }

    /// <summary>
    /// A SetFieldDefinition that uses a field and an AggregateExpression to define the field to be set.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    public class AggregateExpressionSetFieldDefinition<TDocument, TField> : SetFieldDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument, TField> _field;
        private readonly AggregateExpressionDefinition<TDocument, TField> _value;

        /// <summary>
        /// Initializes an instance of AggregateExpressionSetFieldDefinition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        public AggregateExpressionSetFieldDefinition(FieldDefinition<TDocument, TField> field, AggregateExpressionDefinition<TDocument, TField> value)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _value = Ensure.IsNotNull(value, nameof(value));
        }

        /// <inheritdoc/>
        public override BsonElement Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry, linqProvider);
            var renderedValue = _value.Render(documentSerializer, serializerRegistry, linqProvider);
            return new BsonElement(renderedField.FieldName, renderedValue);
        }
    }

    /// <summary>
    /// A SetFieldDefinition that uses a field and a a constant to define the field to be set.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    /// <typeparam name="TField">The type of the field.</typeparam>
    public sealed class ConstantSetFieldDefinition<TDocument, TField> : SetFieldDefinition<TDocument>
    {
        // private fields
        private readonly FieldDefinition<TDocument, TField> _field;
        private readonly TField _value;

        // public constructors
        /// <summary>
        /// Initializes an instance of ConstantSetFieldDefinition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        public ConstantSetFieldDefinition(FieldDefinition<TDocument, TField> field, TField value)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _value = value;
        }

        // public methods
        /// <inheritdoc/>
        public override BsonElement Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry, LinqProvider linqProvider)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry, linqProvider);
            var serializedValue = SerializationHelper.SerializeValue(renderedField.ValueSerializer, _value);

            return new BsonElement(renderedField.FieldName, serializedValue);
        }
    }
}
