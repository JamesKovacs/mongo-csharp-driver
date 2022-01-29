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
using MongoDB.Driver.Tests.Linq.Linq3ImplementationTests;
using Xunit;

namespace MongoDB.Driver.Tests.Jira
{
    public class CSharp3225Tests
    {
        // these examples are taken from: https://docs.mongodb.com/manual/reference/operator/aggregation/setWindowFields/#examples

        [Fact]
        public void Use_documents_window_to_obtain_cumulative_quantity_for_each_state_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.State,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.OrderDate),
                    output: p => new { CumulativeQuantityForState = p.Sum(x => x.Quantity, WindowBoundaries.Documents(WindowBoundary.Unbounded, WindowBoundary.Current)) });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : '$State',
                        sortBy : { OrderDate : 1 },
                        output : {
                            CumulativeQuantityForState : {
                                $sum : '$Quantity',
                                window : {
                                    documents : ['unbounded', 'current']
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(4, 0, 2, 5, 3, 1);
            results.Select(s => s["CumulativeQuantityForState"].AsInt32).Should().Equal(162, 282, 427, 134, 238, 378);
        }

        [Fact]
        public void Use_documents_window_to_obtain_cumulative_quantity_for_each_year_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.OrderDate.Year,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.OrderDate),
                    output: p => new { CumulativeQuantityForYear = p.Sum(x => x.Quantity, WindowBoundaries.Documents(WindowBoundary.Unbounded, WindowBoundary.Current)) });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : { $year : '$OrderDate' } ,
                        sortBy : { OrderDate : 1 },
                        output : {
                            CumulativeQuantityForYear : {
                                $sum : '$Quantity',
                                window : {
                                    documents : ['unbounded', 'current']
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(5, 4, 3, 0, 2, 1);
            results.Select(s => s["CumulativeQuantityForYear"].AsInt32).Should().Equal(134, 296, 104, 224, 145, 285);
        }

        [Fact]
        public void Use_documents_window_to_obtain_moving_average_quantity_for_each_year_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.OrderDate.Year,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.OrderDate),
                    output: p => new { CumulativeQuantityForYear = p.Average(x => x.Quantity, WindowBoundaries.Documents(-1, 0)) });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : { $year : '$OrderDate' } ,
                        sortBy : { OrderDate : 1 },
                        output : {
                            AverageQuantity : {
                                $avg : '$Quantity',
                                window : {
                                    documents : [-1, 0]
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(5, 4, 3, 0, 2, 1);
            results.Select(s => s["AverageQuantity"].AsDouble).Should().Equal(134, 148, 104, 112, 145, 142.5);
        }

        [Fact]
        public void Use_documents_window_to_obtain_cumulative_and_maximum_quantity_for_each_year_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.OrderDate.Year,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.OrderDate),
                    output: p => new
                    {
                        CumulativeQuantityForYear = p.Sum(x => x.Quantity, WindowBoundaries.Documents(WindowBoundary.Unbounded, WindowBoundary.Current)),
                        MaximumQuantityForYear = p.Max(x => x.Quantity, WindowBoundaries.Documents(WindowBoundary.Unbounded, WindowBoundary.Unbounded)),
                    });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : { $year : '$OrderDate' } ,
                        sortBy : { OrderDate : 1 },
                        output : {
                            CumulativeQuantityForYear : {
                                $sum : '$Quantity',
                                window : {
                                    documents : ['unbounded', 'current']
                                }
                            },
                            MaxQuantityForYear : {
                                $max : '$Quantity',
                                window : {
                                    documents : ['unbounded', 'unbounded']
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(5, 4, 3, 0, 2, 1);
            results.Select(s => s["CumulativeQuantityForYear"].AsInt32).Should().Equal(134, 296, 104, 224, 145, 285);
            results.Select(s => s["MaxQuantityForYear"].AsInt32).Should().Equal(162, 162, 120, 120, 145, 145);
        }

        [Fact]
        public void Range_window_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.State,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.Price),
                    output: p => new { QuantityFromSimilarOrders = p.Sum(x => x.Quantity, WindowBoundaries.Range(-10, 10)) });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : '$State',
                        sortBy : { Price : 1 },
                        output : {
                            QuantityFromSimilarOrders : {
                                $sum : '$Quantity',
                                window : {
                                    range : [-10, 10]
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(2, 0, 4, 3, 1, 5);
            results.Select(s => s["QuantityFromSimilarOrders"].AsInt32).Should().Equal(265, 265, 162, 244, 244, 134);
        }

        [Fact]
        public void Use_a_time_range_window_with_a_positive_upper_bound_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.State,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.OrderDate),
                    output: p => new { RecentOrders = p.Push(x => x.OrderDate, WindowBoundaries.Range(WindowBoundary.Unbounded, WindowBoundary.Months(10))) });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : '$State',
                        sortBy : { OrderDate : 1 },
                        output : {
                            RecentOrders : {
                                $push : '$OrderDate',
                                window : {
                                    range : ['unbounded', 10],
                                    unit : 'month'
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(4, 0, 2, 5, 3, 1);
            results[0]["RecentOrders"].AsBsonArray.Should().Equals(new[] { "2019-05-18T16:09:01Z" }.Select(s => DateTime.Parse(s)));
            results[1]["RecentOrders"].AsBsonArray.Should().Equals(new[] { "2019-05-18T16:09:01Z", "2020-05-18T14:10:30Z", "2021-01-11T06:31:15Z" }.Select(s => DateTime.Parse(s)));
            results[2]["RecentOrders"].AsBsonArray.Should().Equals(new[] { "2019-05-18T16:09:01Z", "2020-05-18T14:10:30Z", "2021-01-11T06:31:15Z" }.Select(s => DateTime.Parse(s)));
            results[3]["RecentOrders"].AsBsonArray.Should().Equals(new[] { "2019-01-08T06:12:03Z" }.Select(s => DateTime.Parse(s)));
            results[4]["RecentOrders"].AsBsonArray.Should().Equals(new[] { "2019-01-08T06:12:03Z", "2020-02-08T13:13:23Z" }.Select(s => DateTime.Parse(s)));
            results[5]["RecentOrders"].AsBsonArray.Should().Equals(new[] { "2019-01-08T06:12:03Z", "2020-02-08T13:13:23Z", "2021-03-20T11:30:05Z" }.Select(s => DateTime.Parse(s)));
        }

        [Fact]
        public void Use_a_time_range_window_with_a_negative_upper_bound_example_should_work()
        {
            var collection = Setup();

            var aggregate = collection
                .Aggregate()
                .SetWindowFields(
                    partitionBy: x => x.State,
                    sortBy: Builders<CakeSales>.Sort.Ascending(x => x.OrderDate),
                    output: p => new { RecentOrders = p.Push(x => x.OrderDate, WindowBoundaries.Range(WindowBoundary.Unbounded, WindowBoundary.Months(-10))) });

            var stages = Linq3TestHelpers.Translate(collection, aggregate);
            var expectedStages = new[]
            {
                @"
                {
                    $setWindowFields : {
                        partitionBy : '$State',
                        sortBy : { OrderDate : 1 },
                        output : {
                            RecentOrders : {
                                $push : '$OrderDate',
                                window : {
                                    range : ['unbounded', -10],
                                    unit : 'month'
                                }
                            }
                        }
                    }
                }
                "
            };
            Linq3TestHelpers.AssertStages(stages, expectedStages);

            var results = aggregate.ToList();
            results.Count.Should().Be(6);
            results.Select(s => s["_id"].AsInt32).Should().Equal(4, 0, 2, 5, 3, 1);
            results[0]["RecentOrders"].AsBsonArray.Values.Should().BeEmpty();
            results[1]["RecentOrders"].AsBsonArray.Values.Should().Equal(new[] { "2020-05-18T14:10:30Z" }.Select(s => DateTime.Parse(s)));
            results[2]["RecentOrders"].AsBsonArray.Values.Should().Equal(new[] { "2019-05-18T16:09:01Z" }.Select(s => DateTime.Parse(s)));
            results[3]["RecentOrders"].AsBsonArray.Values.Should().BeEmpty();
            results[4]["RecentOrders"].AsBsonArray.Values.Should().Equal(new[] { "2019-01-08T06:12:03Z" }.Select(s => DateTime.Parse(s)));
            results[5]["RecentOrders"].AsBsonArray.Values.Should().Equal(new[] { "2019-01-08T06:12:03Z", "2020-02-08T13:13:23Z" }.Select(s => DateTime.Parse(s)));
        }

        private IMongoCollection<CakeSales> Setup()
        {
            var client = DriverTestConfiguration.Linq3Client;
            var database = client.GetDatabase("test");
            var collection = database.GetCollection<CakeSales>("cakeSales");

            database.DropCollection("cakeSales");
            collection.InsertMany(new[]
            {
                new CakeSales { Id = 0, Type = "chocolate", OrderDate = DateTime.Parse("2020-05-18T14:10:30Z"), State = "CA", Price = 13.00, Quantity = 120 },
                new CakeSales { Id = 1, Type = "chocolate", OrderDate = DateTime.Parse("2021-03-20T11:30:05Z"), State = "WA", Price = 14.00, Quantity = 140 },
                new CakeSales { Id = 2, Type = "vanilla", OrderDate = DateTime.Parse("2021-01-11T06:31:15Z"), State = "CA", Price = 12.00, Quantity = 145 },
                new CakeSales { Id = 3, Type = "vanilla", OrderDate = DateTime.Parse("2020-02-08T13:13:23Z"), State = "WA", Price = 13.00, Quantity = 104 },
                new CakeSales { Id = 4, Type = "strawberry", OrderDate = DateTime.Parse("2019-05-18T16:09:01Z"), State = "CA", Price = 41.00, Quantity = 162 },
                new CakeSales { Id = 5, Type = "strawberry", OrderDate = DateTime.Parse("2019-01-08T06:12:03Z"), State = "WA", Price = 43.00, Quantity = 134 }
            });

            return collection;
        }

        public class CakeSales
        {
            public int Id { get; set; }
            public string Type { get; set; }
            public DateTime OrderDate { get; set; }
            public string State { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
