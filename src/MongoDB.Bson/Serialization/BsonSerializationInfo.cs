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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents the information needed to serialize a member.
    /// </summary>
    public class BsonSerializationInfo
    {
        #region static
        /// <summary>
        /// Creates anew instance of the BsonSerializationinfo class with an element path instead of an element name.
        /// </summary>
        /// <param name="elementPath">The element path.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <returns>A BsonSerializationInfo.</returns>
        public static BsonSerializationInfo CreateWithPath(IEnumerable<string> elementPath, IBsonSerializer serializer, Type nominalType)
        {
            return new BsonSerializationInfo(elementPath.ToList(), serializer, nominalType);
        }
        #endregion

        // private fields
        private readonly string _elementName;
        private readonly IReadOnlyList<string> _elementPath;
        private readonly IBsonSerializer _serializer;
        private readonly Type _nominalType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonSerializationInfo class.
        /// </summary>
        /// <param name="elementName">The element name.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="nominalType">The nominal type.</param>
        public BsonSerializationInfo(string elementName, IBsonSerializer serializer, Type nominalType)
        {
            _elementName = elementName;
            _serializer = serializer;
            _nominalType = nominalType;
        }

        private BsonSerializationInfo(IReadOnlyList<string> elementPath, IBsonSerializer serializer, Type nominalType)
        {
            _elementPath = elementPath;
            _serializer = serializer;
            _nominalType = nominalType;
        }

        // public properties
        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string ElementName
        {
            get
            {
                if (_elementPath != null)
                {
                    throw new InvalidOperationException("When ElementPath is not null you must use it instead.");
                }
                return _elementName;
            }
        }

        /// <summary>
        /// Gets element path.
        /// </summary>
        public IReadOnlyList<string> ElementPath
        {
            get { return _elementPath; }
        }

        /// <summary>
        /// Gets or sets the serializer.
        /// </summary>
        public IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        /// <summary>
        /// Gets or sets the nominal type.
        /// </summary>
        public Type NominalType
        {
            get { return _nominalType; }
        }

        /// <summary>
        /// Deserializes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A deserialized value.</returns>
        public object DeserializeValue(BsonValue value)
        {
            var tempDocument = new BsonDocument("value", value);
            using (var reader = new BsonDocumentReader(tempDocument))
            {
                var context = BsonDeserializationContext.CreateRoot(reader);
                reader.ReadStartDocument();
                reader.ReadName("value");
                var deserializedValue = _serializer.Deserialize(context);
                reader.ReadEndDocument();
                return deserializedValue;
            }
        }

        /// <summary>
        /// Merges the new BsonSerializationInfo by taking its properties and concatenating its ElementName.
        /// </summary>
        /// <param name="newSerializationInfo">The new info.</param>
        /// <returns>A new BsonSerializationInfo.</returns>
        [Obsolete("This method is no longer relevant because field names are now allowed to contain dots.")]
        public BsonSerializationInfo Merge(BsonSerializationInfo newSerializationInfo)
        {
            string elementName = null;
            if (ElementName != null && newSerializationInfo.ElementName != null)
            {
                elementName = ElementName + "." + newSerializationInfo.ElementName;
            }
            else if (ElementName != null)
            {
                elementName = ElementName;
            }
            else if (newSerializationInfo.ElementName != null)
            {
                elementName = newSerializationInfo.ElementName;
            }

            return new BsonSerializationInfo(
                elementName,
                newSerializationInfo._serializer,
                newSerializationInfo._nominalType);
        }

        /// <summary>
        /// Serializes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The serialized value.</returns>
        public BsonValue SerializeValue(object value)
        {
            var tempDocument = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(tempDocument))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("value");
                _serializer.Serialize(context, value);
                bsonWriter.WriteEndDocument();
                return tempDocument[0];
            }
        }

        /// <summary>
        /// Serializes the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns>The serialized values.</returns>
        public BsonArray SerializeValues(IEnumerable values)
        {
            var tempDocument = new BsonDocument();
            using (var bsonWriter = new BsonDocumentWriter(tempDocument))
            {
                var context = BsonSerializationContext.CreateRoot(bsonWriter);
                bsonWriter.WriteStartDocument();
                bsonWriter.WriteName("values");
                bsonWriter.WriteStartArray();
                foreach (var value in values)
                {
                    _serializer.Serialize(context, value);
                }
                bsonWriter.WriteEndArray();
                bsonWriter.WriteEndDocument();

                return tempDocument[0].AsBsonArray;
            }
        }

        /// <summary>
        /// Creates a new BsonSerializationInfo object using the elementName provided and copying all other attributes.
        /// </summary>
        /// <param name="elementName">Name of the element.</param>
        /// <returns>A new BsonSerializationInfo.</returns>
        public BsonSerializationInfo WithNewName(string elementName)
        {
            return new BsonSerializationInfo(
                elementName,
                _serializer,
                _nominalType);
        }
    }
}
