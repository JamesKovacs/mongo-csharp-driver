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
using System.Linq;
using FluentAssertions;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.TestHelpers.XunitExtensions;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class CSharpxxxxTests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Find_with_unsupported_projection_should_throw(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            RequireServer.Check().DoesNotSupport(Feature.FindProjectionExpressions);
            var collection = GetCollection(linqProvider);

            var find = collection.Find("{}")
                .Project(x => new { Y = x.X });

            var projection = TranslateFindProjection(collection, find);

            if (linqProvider == LinqProvider.V2)
            {
                projection.Should().Be("{ X : 1, _id : 0 }");

                var results = find.ToList();
                results.Select(x => x.Y).Should().Equal(1);
            }
            else
            {
                projection.Should().Be("{ Y : '$X', _id : 0 }");

                var exception = Record.Exception(() =>  find.ToList());
                exception.Should().BeOfType<NotSupportedException>();
                exception.Message.Should().Be("The projection specification { \"Y\" : \"$X\" } is not supported with find on servers prior to version 4.4.");
            }
        }

        private IMongoCollection<C> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<C>("test", linqProvider);
            CreateCollection(
                collection,
                new C { Id = 1, X = 1 });
            return collection;
        }

        private class C
        {
            public int Id { get; set; }
            public int X { get; set; }
        }
    }
}
