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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Tests.Linq.Linq3ImplementationTests;
using Xunit;

namespace MongoDB.Driver.Tests.Jira
{
    public class CSharp1950Tests : Linq3IntegrationTest
    {
        [Fact]
        public void StringIn_with_string_field_name_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.StringIn("S", new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"S\" : { \"$in\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("a1", "a2", "b3");
        }

        [Fact]
        public void StringIn_with_string_field_expression_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.StringIn(x => x.S, new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"S\" : { \"$in\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("a1", "a2", "b3");
        }

        [Fact]
        public void StringIn_with_string_array_field_name_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.StringIn("SA", new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"SA\" : { \"$in\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("a1", "a2", "b3");
        }

        [Fact]
        public void StringArrayIn_with_string_array_field_name_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.AnyStringIn("SA", new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"SA\" : { \"$in\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("a1", "a2", "b3");
        }

        [Fact]
        public void StringArrayIn_with_string_array_field_expression_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.AnyStringIn(x => x.SA, new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"SA\" : { \"$in\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("a1", "a2", "b3");
        }

        [Fact]
        public void StringNin_with_string_field_name_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.StringNin("S", new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"S\" : { \"$nin\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("b4");
        }

        [Fact]
        public void StringNin_with_string_field_expression_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.StringNin(x => x.S, new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"S\" : { \"$nin\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("b4");
        }

        [Fact]
        public void StringNin_with_string_array_field_name_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.StringNin("SA", new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"SA\" : { \"$nin\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("b4");
        }

        [Fact]
        public void StringArrayNin_with_string_array_field_name_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.AnyStringNin("SA", new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"SA\" : { \"$nin\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("b4");
        }

        [Fact]
        public void StringArrayNin_with_string_array_field_expression_should_work()
        {
            var collection = CreateCollection();
            var builder = Builders<C>.Filter;
            var filter = builder.AnyStringNin(x => x.SA, new StringOrRegularExpression[] { new BsonRegularExpression("^a"), "b3" });

            var registry = BsonSerializer.SerializerRegistry;
            var serializer = registry.GetSerializer<C>();
            var rendered = filter.Render(serializer, registry, LinqProvider.V3).ToJson();
            rendered.Should().Be("{ \"SA\" : { \"$nin\" : [/^a/, \"b3\"] } }");

            var results = collection.FindSync(filter).ToList().OrderBy(x => x.Id).ToList();
            results.Select(x => x.S).Should().Equal("b4");
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();
            CreateCollection(
                collection,
                new C { Id = 1, S = "a1", SA = new[] { "a1" } },
                new C { Id = 2, S = "a2", SA = new[] { "a2" } },
                new C { Id = 3, S = "b3", SA = new[] { "b3" } },
                new C { Id = 4, S = "b4", SA = new[] { "b4" } });
            return collection;
        }

        public class C
        {
            public int Id { get; set; }
            public string S { get; set; }
            public string[] SA { get; set; }
        }
    }
}
