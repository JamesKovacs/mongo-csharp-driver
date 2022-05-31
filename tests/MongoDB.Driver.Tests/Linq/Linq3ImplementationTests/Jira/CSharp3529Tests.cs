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
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp3529Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Bottom_find_the_bottom_score_in_a_single_game_example_should_work_using_GroupBy_with_result_selector()
        {
            var collection = CreateGameScoreCollection();
            var queryable = collection
                .AsQueryable()
                .Where(x => x.GameId == "G1")
                .GroupBy(
                    x => x.GameId,
                    (key, elements) =>
                        new
                        {
                            Id = key,
                            PlayerId = elements.Bottom(
                                Builders<GameScore>.Sort.Descending(g => g.Score),
                                e => new { P = e.PlayerId, S = e.Score })
                        });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $match : { GameId : 'G1' } }",
                "{ $group : { _id : '$GameId', __agg0 : { $bottom : { sortBy : { Score : -1 }, output : { P : '$PlayerId', S : '$Score' } } } } }",
                "{ $project : { Id : '$_id', PlayerId : '$__agg0', _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new { Id = "G1", PlayerId = new { P = "PlayerD", S = 1 } });
        }

        [Fact]
        public void Bottom_find_the_bottom_score_in_a_single_game_example_should_work_using_GroupBy_and_Select()
        {
            var collection = CreateGameScoreCollection();
            var queryable = collection
                .AsQueryable()
                .Where(x => x.GameId == "G1")
                .GroupBy(x => x.GameId)
                .Select(
                    g =>
                        new
                        {
                            Id = g.Key,
                            PlayerId = g.Bottom(
                                Builders<GameScore>.Sort.Descending(g => g.Score),
                                e => new { P = e.PlayerId, S = e.Score })
                        });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $match : { GameId : 'G1' } }",
                "{ $group : { _id : '$GameId', __agg0 : { $bottom : { sortBy : { Score : -1 }, output : { P : '$PlayerId', S : '$Score' } } } } }",
                "{ $project : { Id : '$_id', PlayerId : '$__agg0', _id : 0 } }");

            var results = queryable.ToList();
            results.Should().Equal(new { Id = "G1", PlayerId = new { P = "PlayerD", S = 1 } });
        }

        [Fact]
        public void Bottom_find_the_bottom_score_across_multiple_games_example_should_work_using_GroupBy_with_result_selector()
        {
            var collection = CreateGameScoreCollection();
            var queryable = collection
                .AsQueryable()
                .GroupBy(
                    x => x.GameId,
                    (key, elements) =>
                        new
                        {
                            Id = key,
                            PlayerId = elements.Bottom(
                                Builders<GameScore>.Sort.Descending(g => g.Score),
                                e => new { P = e.PlayerId, S = e.Score })
                        });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $group : { _id : '$GameId', __agg0 : { $bottom : { sortBy : { Score : -1 }, output : { P : '$PlayerId', S : '$Score' } } } } }",
                "{ $project : { Id : '$_id', PlayerId : '$__agg0' , _id : 0 } }");

            var results = queryable.ToList().OrderBy(x => x.Id).ToList();
            results.Should().Equal(
                new { Id = "G1", PlayerId = new { P = "PlayerD", S = 1 } },
                new { Id = "G2", PlayerId = new { P = "PlayerA", S = 10 } }
            );
        }

        [Fact]
        public void Bottom_find_the_bottom_score_across_multiple_games_example_should_work_using_GroupBy_and_Select()
        {
            var collection = CreateGameScoreCollection();
            var queryable = collection
                .AsQueryable()
                .GroupBy(x => x.GameId)
                .Select(
                    g =>
                        new
                        {
                            Id = g.Key,
                            PlayerId = g.Bottom(
                                Builders<GameScore>.Sort.Descending(g => g.Score),
                                e => new { P = e.PlayerId, S = e.Score })
                        });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $group : { _id : '$GameId', __agg0 : { $bottom : { sortBy : { Score : -1 }, output : { P : '$PlayerId', S : '$Score' } } } } }",
                "{ $project : { Id : '$_id', PlayerId : '$__agg0' , _id : 0 } }");

            var results = queryable.ToList().OrderBy(x => x.Id).ToList();
            results.Should().Equal(
                new { Id = "G1", PlayerId = new { P = "PlayerD", S = 1 } },
                new { Id = "G2", PlayerId = new { P = "PlayerA", S = 10 } }
            );
        }

        [Fact]
        public void BottomN_find_the_three_lowest_scores_in_a_single_game_example_should_work_using_GroupBy_with_result_selector()
        {
            var collection = CreateGameScoreCollection();
            var queryable = collection
                .AsQueryable()
                .Where(x => x.GameId == "G1")
                .GroupBy(
                    x => x.GameId,
                    (key, elements) =>
                        new
                        {
                            Id = key,
                            PlayerId = elements.BottomN(
                                Builders<GameScore>.Sort.Descending(g => g.Score),
                                e => new { P = e.PlayerId, S = e.Score },
                                3)
                        });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $match : { GameId : 'G1' } }",
                "{ $group : { _id : '$GameId', __agg0 : { $bottomN : { sortBy : { Score : -1 }, output : { P : '$PlayerId', S : '$Score' }, n : 3 } } } }",
                "{ $project : { Id : '$_id', PlayerId : '$__agg0', _id : 0 } }");

            var results = queryable.ToList();
            results.Should().HaveCount(1);
            results[0].Id.Should().Be("G1");
            results[0].PlayerId.Should().Equal(new { P = "PlayerB", S = 33 }, new { P = "PlayerA", S = 31 }, new { P = "PlayerD", S = 1 });
        }

        [Fact]
        public void BottomN_find_the_three_lowest_scores_in_a_single_game_example_should_work_using_GroupBy_and_Select()
        {
            var collection = CreateGameScoreCollection();
            var queryable = collection
                .AsQueryable()
                .Where(x => x.GameId == "G1")
                .GroupBy(x => x.GameId)
                .Select(
                    g =>
                        new
                        {
                            Id = g.Key,
                            PlayerId = g.BottomN(
                                Builders<GameScore>.Sort.Descending(g => g.Score),
                                e => new { P = e.PlayerId, S = e.Score },
                                3)
                        });

            var stages = Translate(collection, queryable);
            AssertStages(
                stages,
                "{ $match : { GameId : 'G1' } }",
                "{ $group : { _id : '$GameId', __agg0 : { $bottomN : { sortBy : { Score : -1 }, output : { P : '$PlayerId', S : '$Score' }, n : 3 } } } }",
                "{ $project : { Id : '$_id', PlayerId : '$__agg0', _id : 0 } }");

            var results = queryable.ToList();
            results.Should().HaveCount(1);
            results[0].Id.Should().Be("G1");
            results[0].PlayerId.Should().Equal(new { P = "PlayerB", S = 33 }, new { P = "PlayerA", S = 31 }, new { P = "PlayerD", S = 1 });
        }

        private List<TAnonymous> CreateList<TAnonymous>(params TAnonymous[] items)
        {
            return new List<TAnonymous>(items);
        }

        private IMongoCollection<GameScore> CreateGameScoreCollection()
        {
            var collection = GetCollection<GameScore>();

            CreateCollection(
                collection,
                new GameScore { Id = 1, PlayerId = "PlayerA", GameId = "G1", Score = 31 },
                new GameScore { Id = 2, PlayerId = "PlayerB", GameId = "G1", Score = 33 },
                new GameScore { Id = 3, PlayerId = "PlayerC", GameId = "G1", Score = 99 },
                new GameScore { Id = 4, PlayerId = "PlayerD", GameId = "G1", Score = 1 },
                new GameScore { Id = 5, PlayerId = "PlayerA", GameId = "G2", Score = 10 },
                new GameScore { Id = 6, PlayerId = "PlayerB", GameId = "G2", Score = 14 },
                new GameScore { Id = 7, PlayerId = "PlayerC", GameId = "G2", Score = 66 },
                new GameScore { Id = 8, PlayerId = "PlayerD", GameId = "G2", Score = 80 }
                );

            return collection;
        }

        public class GameScore
        {
            public int Id { get; set; }
            public string PlayerId { get; set; }
            public string GameId { get; set; }
            public int Score { get; set; }
        }
    }
}
