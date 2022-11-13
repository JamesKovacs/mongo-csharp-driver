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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Bson.Tests.Jira.CSharp783
{
    public class CSharp4412Tests
    {
        [Fact]
        public void IEnumerable_Ids_should_deserialize_from_ObjectIds()
        {
            var json = "{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }";

            var rehydrated = BsonSerializer.Deserialize<C>(json);

            rehydrated.Ids.Should().Equal("0102030405060708090a0b0c");
        }

        [Fact]
        public void IEnumerable_with_Array_of_string_should_serialize_as_ObjectIds()
        {
            var document = new C { Ids = new string[] { "0102030405060708090a0b0c" } };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IEnumerable_with_List_of_string_should_serialize_as_ObjectIds()
        {
            var document = new C { Ids = new List<string> { "0102030405060708090a0b0c" } };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IEnumerable_with_Collection_of_string_should_serialize_as_ObjectIds()
        {
            var document = new C { Ids = new Collection<string> { "0102030405060708090a0b0c" } };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IEnumerable_with_ReadOnlyCollection_of_string_should_serialize_as_ObjectIds()
        {
            var document = new C { Ids = new ReadOnlyCollection<string>(new List<string> { "0102030405060708090a0b0c" }) };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IList_Ids_should_deserialize_from_ObjectIds()
        {
            var json = "{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }";

            var rehydrated = BsonSerializer.Deserialize<D>(json);

            rehydrated.Ids.Should().Equal("0102030405060708090a0b0c");
        }

        [Fact]
        public void IList_with_Array_of_string_should_serialize_as_ObjectIds()
        {
            var document = new D { Ids = new string[] { "0102030405060708090a0b0c" } };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IList_with_List_of_string_should_serialize_as_ObjectIds()
        {
            var document = new D { Ids = new List<string> { "0102030405060708090a0b0c" } };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IList_with_Collection_of_string_should_serialize_as_ObjectIds()
        {
            var document = new D { Ids = new Collection<string> { "0102030405060708090a0b0c" } };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        [Fact]
        public void IList_with_ReadOnlyCollection_of_string_should_serialize_as_ObjectIds()
        {
            var document = new D { Ids = new ReadOnlyCollection<string>(new List<string> { "0102030405060708090a0b0c" }) };

            var json = document.ToJson();

            json.Should().Be("{ \"Ids\" : [ObjectId(\"0102030405060708090a0b0c\")] }");
        }

        private class C
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public IEnumerable<string> Ids { get; set; }
        }

        private class D
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public IList<string> Ids { get; set; }
        }

        public enum E { A, B }
    }
}
