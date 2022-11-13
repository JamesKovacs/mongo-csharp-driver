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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for IEnumerable and any other interface implemented by List.
    /// </summary>
    /// <typeparam name="TEnumerable">The type of an IEnumerable interface.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public class IEnumerableListSerializer<TEnumerable, TItem> :
        SerializerBase<TEnumerable>,
        IBsonArraySerializer,
        IChildSerializerConfigurable
        where TEnumerable : class, IEnumerable<TItem>
    {
        // private fields
        private readonly Lazy<IBsonSerializer<TItem>> _lazyItemSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the IEnumerableSerializer class.
        /// </summary>
        public IEnumerableListSerializer()
            : this(BsonSerializer.SerializerRegistry)
        {
        }

        /// <summary>
        /// Initializes a new instance of the IEnumerableSerializer class.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        public IEnumerableListSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            if (itemSerializer == null)
            {
                throw new ArgumentNullException("itemSerializer");
            }

            _lazyItemSerializer = new Lazy<IBsonSerializer<TItem>>(() => itemSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the IEnumerableSerializer class.
        /// </summary>
        /// <param name="serializerRegistry">The serializer registry.</param>
        public IEnumerableListSerializer(IBsonSerializerRegistry serializerRegistry)
        {
            if (serializerRegistry == null)
            {
                throw new ArgumentNullException("serializerRegistry");
            }

            _lazyItemSerializer = new Lazy<IBsonSerializer<TItem>>(() => serializerRegistry.GetSerializer<TItem>());
        }

        // public properties
        /// <summary>
        /// Gets the item serializer.
        /// </summary>
        /// <value>
        /// The item serializer.
        /// </value>
        public IBsonSerializer<TItem> ItemSerializer
        {
            get { return _lazyItemSerializer.Value; }
        }

        // public methods
        /// <inheritdoc/>
        public override TEnumerable Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            if (reader.GetCurrentBsonType() == BsonType.Null)
            {
                reader.ReadNull();
                return null;
            }
            else
            {
                var list = new List<TItem>();
                reader.ReadStartArray();
                while (reader.ReadBsonType() != 0)
                {
                    var item = _lazyItemSerializer.Value.Deserialize(context);
                    list.Add(item);
                }
                reader.ReadEndArray();
                return (TEnumerable)(object)list;
            }
        }

        /// <inheritdoc/>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TEnumerable value)
        {
            var writer = context.Writer;

            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteStartArray();
                foreach (var item in value)
                {
                    _lazyItemSerializer.Value.Serialize(context, item);
                }
                writer.WriteEndArray();
            }
        }

        /// <summary>
        /// Tries to get the serialization info for the individual items of the array.
        /// </summary>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        /// The serialization info for the items.
        /// </returns>
        public bool TryGetItemSerializationInfo(out BsonSerializationInfo serializationInfo)
        {
            var serializer = _lazyItemSerializer.Value;
            serializationInfo = new BsonSerializationInfo(null, serializer, serializer.ValueType);
            return true;
        }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified item serializer.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <returns>The reconfigured serializer.</returns>
        public IEnumerableListSerializer<TEnumerable, TItem> WithItemSerializer(IBsonSerializer<TItem> itemSerializer)
        {
            return new IEnumerableListSerializer<TEnumerable, TItem>(itemSerializer);
        }

        // explicit interface implementations
        IBsonSerializer IChildSerializerConfigurable.ChildSerializer
        {
            get { return ItemSerializer; }
        }

        IBsonSerializer IChildSerializerConfigurable.WithChildSerializer(IBsonSerializer childSerializer)
        {
            return WithItemSerializer((IBsonSerializer<TItem>)childSerializer);
        }
    }
}
