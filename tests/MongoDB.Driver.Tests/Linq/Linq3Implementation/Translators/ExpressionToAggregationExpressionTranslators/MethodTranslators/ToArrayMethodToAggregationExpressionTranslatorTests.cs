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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators.MethodTranslators
{
    public class ToArrayMethodToAggregationExpressionTranslatorTests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Array_ToArray_should_work(
            [Values(false, true)] bool withNestedAsQueryable,
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);

            var queryable = withNestedAsQueryable ?
                collection.AsQueryable().Select(x => x.Array.AsQueryable().ToArray()) :
                collection.AsQueryable().Select(x => x.Array.ToArray());

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, queryable));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, queryable);
                AssertStages(stages, "{ $project : { _v : '$Array', _id : 0 } }");

                var result = queryable.Single();
                result.Should().Equal(1, 2, 3);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void IEnumerable_ToArray_should_work(
            [Values(false, true)] bool withNestedAsQueryable,
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);

            var queryable = withNestedAsQueryable ?
                collection.AsQueryable().Select(x => x.IEnumerable.AsQueryable().ToArray()) :
                collection.AsQueryable().Select(x => x.IEnumerable.ToArray());

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, queryable));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, queryable);
                AssertStages(stages, "{ $project : { _v : '$IEnumerable', _id : 0 } }");

                var result = queryable.Single();
                result.Should().Equal(1, 2, 3);
            }
        }

        [Theory]
        [ParameterAttributeData]
        public void List_ToArray_should_work(
            [Values(false, true)] bool withNestedAsQueryable,
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);

            var queryable = withNestedAsQueryable ?
                collection.AsQueryable().Select(x => x.List.AsQueryable().ToArray()) :
                collection.AsQueryable().Select(x => x.List.ToArray());

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, queryable));
                exception.Should().BeOfType<NotSupportedException>();
            }
            else
            {
                var stages = Translate(collection, queryable);
                AssertStages(stages, "{ $project : { _v : '$List', _id : 0 } }");

                var result = queryable.Single();
                result.Should().Equal(1, 2, 3);
            }
        }

        private IMongoCollection<C> CreateCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<C>("test", linqProvider);
            CreateCollection(
                collection,
                new C
                {
                    Id = 1,
                    Array = new int[] { 1, 2, 3 },
                    IEnumerable = new List<int> { 1, 2, 3 },
                    List = new List<int> { 1, 2, 3 }
                });
            return collection;
        }

        private class C
        {
            public int Id { get; set; }
            public int[] Array { get; set; }
            public IEnumerable<int> IEnumerable { get; set; }
            public List<int> List { get; set; }
        }
    }
}
