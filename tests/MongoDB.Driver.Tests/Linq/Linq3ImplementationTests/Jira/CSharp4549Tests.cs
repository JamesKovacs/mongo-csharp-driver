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
using System.Linq;
using FluentAssertions;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4549Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Projecting_a_tuple_using_constructor_with_1_item_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int>(x.A));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int>(1));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_2_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int>(x.A, x.B));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int>(1, 2));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_3_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int>(x.A, x.B, x.C));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int>(1, 2, 3));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_4_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int, int>(x.A, x.B, x.C, x.D));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int, int>(1, 2, 3, 4));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_5_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int, int, int>(x.A, x.B, x.C, x.D, x.E));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_6_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int, int, int, int>(x.A, x.B, x.C, x.D, x.E, x.F));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_7_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int, int, int, int, int>(x.A, x.B, x.C, x.D, x.E, x.F, x.G));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_8_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int, int, int, int, int, Tuple<int>>(x.A, x.B, x.C, x.D, x.E, x.F, x.G, new Tuple<int>(x.H)));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G', ['$H']], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int, int, int, int, int, Tuple<int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int>(8)));
        }

        [Fact]
        public void Projecting_a_tuple_using_constructor_with_9_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(x.A, x.B, x.C, x.D, x.E, x.F, x.G, new Tuple<int, int>(x.H, x.I)));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G', ['$H', '$I']], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new Tuple<int, int, int, int, int, int, int, Tuple<int, int>>(1, 2, 3, 4, 5, 6, 7, new Tuple<int, int>(8, 9)));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_1_item_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_2_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_3_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B, x.C));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2, 3));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_4_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B, x.C, x.D));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2, 3, 4));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_5_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B, x.C, x.D, x.E));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2, 3, 4, 5));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_6_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B, x.C, x.D, x.E, x.F));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_7_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B, x.C, x.D, x.E, x.F, x.G));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2, 3, 4, 5, 6, 7));
        }

        [Fact]
        public void Projecting_a_tuple_using_create_with_8_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => Tuple.Create(x.A, x.B, x.C, x.D, x.E, x.F, x.G, x.H));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G', ['$H']], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(Tuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_1_item_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int>(x.A));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int>(1));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_2_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int>(x.A, x.B));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int>(1, 2));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_3_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int>(x.A, x.B, x.C));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int>(1, 2, 3));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_4_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int, int>(x.A, x.B, x.C, x.D));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int, int>(1, 2, 3, 4));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_5_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int, int, int>(x.A, x.B, x.C, x.D, x.E));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int, int, int>(1, 2, 3, 4, 5));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_6_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int, int, int, int>(x.A, x.B, x.C, x.D, x.E, x.F));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int, int, int, int>(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_7_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int, int, int, int, int>(x.A, x.B, x.C, x.D, x.E, x.F, x.G));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_8_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(x.A, x.B, x.C, x.D, x.E, x.F, x.G, new ValueTuple<int>(x.H)));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G', ['$H']], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int>(8)));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_constructor_with_9_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int, int>>(x.A, x.B, x.C, x.D, x.E, x.F, x.G, new ValueTuple<int, int>(x.H, x.I)));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G', ['$H', '$I']], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new ValueTuple<int, int, int, int, int, int, int, ValueTuple<int, int>>(1, 2, 3, 4, 5, 6, 7, new ValueTuple<int, int>(8, 9)));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_1_item_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_2_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_3_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B, x.C));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2, 3));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_4_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B, x.C, x.D));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2, 3, 4));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_5_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B, x.C, x.D, x.E));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2, 3, 4, 5));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_6_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B, x.C, x.D, x.E, x.F));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2, 3, 4, 5, 6));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_7_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B, x.C, x.D, x.E, x.F, x.G));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G'], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7));
        }

        [Fact]
        public void Projecting_a_value_tuple_using_create_with_8_items_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection.AsQueryable()
                .Select(x => ValueTuple.Create(x.A, x.B, x.C, x.D, x.E, x.F, x.G, x.H));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { _v : ['$A', '$B', '$C', '$D', '$E', '$F', '$G', ['$H']], _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(ValueTuple.Create(1, 2, 3, 4, 5, 6, 7, 8));
        }

        private IMongoCollection<T> CreateCollection()
        {
            var collection = GetCollection<T>("test");

            CreateCollection(
                collection,
                new T { Id = 1, A = 1, B = 2, C = 3, D = 4, E = 5, F = 6, G = 7, H = 8, I = 9 });

            return collection;
        }

        private class T
        {
            public int Id { get; set; }
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
            public int D { get; set; }
            public int E { get; set; }
            public int F { get; set; }
            public int G { get; set; }
            public int H { get; set; }
            public int I { get; set; }
        }
    }
}
