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
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4391Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Deserializing_struct_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection
                .AsQueryable();

            var stages = Translate(collection, queryable);
            AssertStages(stages, new string[0]);

            var results = queryable.ToList();
            results.Should().Equal(new S(1, 11, 111), new S(2, 22, 222));
        }

        [Fact]
        public void Deserializing_projected_struct_should_work()
        {
            var collection = CreateCollection();

            var queryable = collection
                .AsQueryable()
                .Select(x => new S(x.Id, x.X * 2, x.Y * 3));

            var stages = Translate(collection, queryable);
            AssertStages(stages, "{ $project : { Id : '$_id', X : { $multiply : ['$X', 2] }, Y : { $multiply : ['$Y', 3] }, _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new S(1, 22, 333), new S(2, 44, 666));
        }

        private IMongoCollection<S> CreateCollection()
        {
            var collection = GetCollection<S>("C");

            CreateCollection(
                collection,
                new S(1, 11, 111),
                new S(2, 22, 222));

            return collection;
        }

        private struct S
        {
            private readonly int _id;
            private readonly int _x;
            private readonly int _y;

            public S(int id, int x, int y)
            {
                _id = id;
                _x = x;
                _y = y;
            }

            public int Id => _id;
            public int X => _x;
            public int Y => _y;
        }
    }
}
