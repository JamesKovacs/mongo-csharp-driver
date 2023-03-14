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

using System.Linq;
using FluentAssertions;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4566Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Filter_with_byte_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.Byte == 1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ Byte : 1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        [Fact]
        public void Filter_with_char_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.Char == 'a');

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ Char : 97 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        [Fact]
        public void Filter_with_char_with_string_representation_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.CharWithStringRepresentation == 'a');

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ CharWithStringRepresentation : 'a' }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        [Fact]
        public void Filter_with_int16_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.Int16 == -1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ Int16 : -1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(2);
        }

        [Fact]
        public void Filter_with_int32_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.Int32 == -1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ Int32 : -1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(2);
        }

        [Fact]
        public void Filter_with_int64_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.Int64 == -1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ Int64 : -1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(2);
        }

        [Fact]
        public void Filter_with_sbyte_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.SByte == -1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ SByte : -1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(2);
        }

        [Fact]
        public void Filter_with_uint16_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.UInt16 == 1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ UInt16 : 1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        [Fact]
        public void Filter_with_uint32_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.UInt32 == 1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ UInt32 : 1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        [Fact]
        public void Filter_with_uint64_should_work()
        {
            var collection = CreateCollection();

            var find = collection
                .Find(x => x.UInt64 == 1);

            var filter = TranslateFindFilter(collection, find);
            filter.Should().Be("{ UInt64 : 1 }");

            var results = find.ToList();
            results.Select(x => x.Id).Should().Equal(1);
        }

        private IMongoCollection<TestObject> CreateCollection()
        {
            var collection = GetCollection<TestObject>("test");

            CreateCollection(
                collection,
                new TestObject
                {
                    Id = 1,

                    Byte = 1,
                    Char = 'a',
                    CharWithStringRepresentation = 'a',
                    Int16 = 1,
                    Int32 = 1,
                    Int64 = 1,
                    SByte = 1,
                    UInt16 = 1,
                    UInt32 = 1,
                    UInt64 = 1,

                    NullableByte = 1,
                    NullableChar = 'a',
                    NullableCharWithStringRepresentation = 'a',
                    NullableInt16 = 1,
                    NullableInt32 = 1,
                    NullableInt64 = 1,
                    NullableSByte = 1,
                    NullableUInt16 = 1,
                    NullableUInt32 = 1,
                    NullableUInt64 = 1,
                },
                new TestObject
                {
                    Id = 2,

                    Int16 = -1,
                    Int32 = -1,
                    Int64 = -1,
                    SByte = -1,

                    NullableInt16 = -1,
                    NullableInt32 = -1,
                    NullableInt64 = -1,
                    NullableSByte = -1,
                },
                new TestObject
                {
                    Id = 3
                });

            return collection;
        }

        private class TestObject
        {
            public int Id { get; set; }

            public byte Byte { get; set; }
            public char Char { get; set; }
            [BsonRepresentation(Bson.BsonType.String)] public char CharWithStringRepresentation { get; set; }
            public short Int16 { get; set; }
            public int Int32 { get; set; }
            public long Int64 { get; set; }
            public sbyte SByte { get; set; }
            public ushort UInt16 { get; set; }
            public uint UInt32 { get; set; }
            public ulong UInt64 { get; set; }

            public byte? NullableByte { get; set; }
            public char? NullableChar { get; set; }
            [BsonRepresentation(Bson.BsonType.String)] public char? NullableCharWithStringRepresentation { get; set; }
            public short? NullableInt16 { get; set; }
            public int? NullableInt32 { get; set; }
            public long? NullableInt64 { get; set; }
            public sbyte? NullableSByte { get; set; }
            public ushort? NullableUInt16 { get; set; }
            public uint? NullableUInt32 { get; set; }
            public ulong? NullableUInt64 { get; set; }
        }
    }
}
