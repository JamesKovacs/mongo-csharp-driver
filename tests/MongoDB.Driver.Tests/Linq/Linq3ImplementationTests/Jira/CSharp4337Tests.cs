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
using System.Linq.Expressions;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4337Tests : Linq3IntegrationTest
    {
        private static (Expression<Func<C, R<bool>>> Projection, string ExpectedStage, bool[] ExpectedResults)[] __predicate_should_use_correct_representation_test_cases = new (Expression<Func<C, R<bool>>> Projection, string ExpectedStage, bool[] ExpectedResults)[]
        {
            (d => new R<bool> { N = d.Id, V = d.E1 == E.A ? true : false }, "{ $project : { N : '$_id', V : { $cond : { if : { $eq : ['$E1', 0] }, then : true, else : false } }, _id : 0 } }", new[] { true, false }),
            (d => new R<bool> { N = d.Id, V = d.S1 == E.A ? true : false }, "{ $project : { N : '$_id', V : { $cond : { if : { $eq : ['$S1', 'A'] }, then : true, else : false } }, _id : 0 } }", new[] { true, false }),
            (d => new R<bool> { N = d.Id, V = E.A == d.E1 ? true : false }, "{ $project : { N : '$_id', V : { $cond : { if : { $eq : [0, '$E1'] }, then : true, else : false } }, _id : 0 } }", new[] { true, false }),
            (d => new R<bool> { N = d.Id, V = E.A == d.S1 ? true : false }, "{ $project : { N : '$_id', V : { $cond : { if : { $eq : ['A', '$S1'] }, then : true, else : false } }, _id : 0 } }", new[] { true, false })
        };

        public static IEnumerable<object[]> Predicate_should_use_correct_representation_member_data()
        {
            var testCases = __predicate_should_use_correct_representation_test_cases;
            for (var i = 0; i < testCases.Length; i++)
            {
                yield return new object[] { i, testCases[i].Projection.ToString(), testCases[i].ExpectedStage, testCases[i].ExpectedResults };
            }
        }

        [Theory]
        [MemberData(nameof(Predicate_should_use_correct_representation_member_data))]
        public void Predicate_should_use_correct_representation(int i, string projectionAsString, string expectedStage, bool[] expectedResults)
        {
            var collection = CreateCollection();
            var projection = __predicate_should_use_correct_representation_test_cases[i].Projection;

            var aggregate = collection.Aggregate()
                .Project(projection);

            var stages = Translate(collection, aggregate);
            AssertStages(stages, expectedStage);

            var results = aggregate.ToList();
            results.OrderBy(r => r.N).Select(r => r.V).Should().Equal(expectedResults);
        }

        private IMongoCollection<C> CreateCollection()
        {
            var collection = GetCollection<C>();

            CreateCollection(
                collection,
                new C { Id = 1, E1 = E.A, E2 = E.A, S1 = E.A, S2 = E.A },
                new C { Id = 2, E1 = E.B, E2 = E.B, S1 = E.B, S2 = E.B });

            return collection;
        }

        public class C
        {
            public int Id { get; set; }

            public E E1 { get; set; }
            public E E2 { get; set; }

            [BsonRepresentation(BsonType.String)] public E S1 { get; set; }
            [BsonRepresentation(BsonType.String)] public E S2 { get; set; }
        }

        public class R<TValue>
        {
            public int N { get; set; }
            public TValue V { get; set; }
        }

        public enum E { A, B, C, D }
    }
}
