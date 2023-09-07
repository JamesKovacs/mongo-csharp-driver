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
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharp4747Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Set_with_one_int_field_name_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set("X", 2);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { X : 2 } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_one_enum_field_name_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set("E", E.A);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { E : 'A' } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_two_int_field_names_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set("X", 2)
                .Set("Z", 4);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { X : 2, Z : 4 } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_two_enum_field_names_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set("E", E.A)
                .Set("F", E.B);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { E : 'A', F : 2 } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_one_int_field_expression_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set(x => x.X, 2);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { X : 2 } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_one_enum_field_expression_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set(x => x.E, E.A);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { E : 'A' } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_two_int_field_expressions_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set(x => x.X, 2)
                .Set(x => x.Y, x => x.X);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { X : 2, Y : '$X' } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_two_enum_field_expressions_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set(x => x.E, E.A)
                .Set(x => x.F, x => x.F);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { E : 'A', F : '$F' } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_two_int_field_expressions_and_one_field_name_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set(x => x.X, 2)
                .Set(x => x.Y, x => x.X)
                .Set("Z", 4);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { X : 2, Y : '$X', Z : 4 } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_two_enum_field_expressions_and_one_field_name_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);
            var fields = Builders<C>.SetFields
                .Set(x => x.E, E.A)
                .Set(x => x.F, x => x.F)
                .Set("G", E.B);

            var aggregate = collection.Aggregate()
                .Set(fields);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $set : { E : 'A', F : '$F', G : 2 } }");
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_new_anonymous_class_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);

            var aggregate = collection.Aggregate()
                .Set(x => new { X = 2, E = E.A, F = x.F, G = E.B, Z = 4 });

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, aggregate));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, aggregate);
                AssertStages(stages, "{ $set : { X : 2, E : 'A', F : '$F', G : 2, Z : 4 } }");
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_new_default_constructor_and_member_initializers_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);

            var aggregate = collection.Aggregate()
                .Set(x => new C { X = 2, E = E.A, F = x.F });

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, aggregate));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, aggregate);
                AssertStages(stages, "{ $set : { X : 2, E : 'A', F : '$F' } }");
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void Set_with_new_copy_constructor_and_member_initializers_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateGetCollection(linqProvider);

            var aggregate = collection.Aggregate()
                .Set(x => new C(x) { X = 2, E = E.A, F = x.F });

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, aggregate));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, aggregate);
                AssertStages(stages, "{ $set : { X : 2, E : 'A', F : '$F' } }");
            }
        }

        private IMongoCollection<C> CreateGetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<C>("test", linqProvider);
            CreateCollection(collection);
            return collection;
        }

        private class C
        {
            public C()
            {
            }

            public C(C c)
            {
                Id = c.Id;
                X = c.X;
                Y = c.Y;
            }

            public int Id { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            [BsonRepresentation(BsonType.String)] public E E { get; set; }
            public E F { get; set; }
        }

        private enum E { A = 1, B };
    }
}
