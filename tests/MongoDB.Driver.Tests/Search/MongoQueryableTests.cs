// Copyright 2010-present MongoDB Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using FluentAssertions;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.Linq;
using Xunit;

namespace MongoDB.Driver.Tests.Search
{
    public class MongoQueryableTests
    {
        private readonly IMongoQueryable<Person> _queryable;

        public MongoQueryableTests()
        {
            var client = DriverTestConfiguration.Linq3Client;
            var db = client.GetDatabase(DriverTestConfiguration.DatabaseNamespace.DatabaseName);
            _queryable = db.GetCollection<Person>(DriverTestConfiguration.CollectionNamespace.CollectionName).AsQueryable();
        }

        [Fact]
        public void Search()
        {
            var query = _queryable
                .Where(x => x.FirstName == "Alexandra")
                .Search(Builders<Person>.Search.Text("Alex", x => x.FirstName))
                .ToString();

            query.Should().EndWith("Aggregate([{ \"$match\" : { \"fn\" : \"Alexandra\" } }, { \"$search\" : { \"text\" : { \"query\" : \"Alex\", \"path\" : \"fn\" } } }])");
        }

        [Fact]
        public void SearchMeta()
        {
            var query = _queryable
                .Where(x => x.FirstName == "Alexandra")
                .SearchMeta(Builders<Person>.Search.Text("Alex", x => x.FirstName))
                .ToString();

            query.Should().EndWith("Aggregate([{ \"$match\" : { \"fn\" : \"Alexandra\" } }, { \"$searchMeta\" : { \"text\" : { \"query\" : \"Alex\", \"path\" : \"fn\" } } }])");
        }

        private class Person
        {
            [BsonElement("fn")]
            public string FirstName { get; set; }

            [BsonElement("ln")]
            public string LastName { get; set; }
        }
    }
}
