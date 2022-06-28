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

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp2107Tests : Linq3IntegrationTest
    {
        [Fact]
        public void Project_should_work()
        {
            var collection = GetCollection<Customer>();

            var aggregate = collection.Aggregate()
                .Project(doc => new
                {
                    UserIsCustomer = doc.Users.Where(user => user.Identity.IdentityType == IdentityType.SomeType)
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { UserIsCustomer : { $filter : { input : '$Users', as : 'user', cond : { $eq : ['$$user.Identity.IdentityType', 'SomeType'] } } }, _id : 0 } }");
        }

        [Fact]
        public void Select_should_work()
        {
            var collection = GetCollection<Customer>();

            var aggregate = collection.AsQueryable()
                .Select(doc => new
                {
                    UserIsCustomer = doc.Users.Where(user => user.Identity.IdentityType == IdentityType.SomeType)
                });

            var stages = Translate(collection, aggregate);
            AssertStages(stages, "{ $project : { UserIsCustomer : { $filter : { input : '$Users', as : 'user', cond : { $eq : ['$$user.Identity.IdentityType', 'SomeType'] } } }, _id : 0 } }");
        }

        private class Customer
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            public IEnumerable<User> Users { get; set; }
        }

        private class User
        {
            public Identity Identity { get; set; }
        }

        private class Identity
        {
            [BsonRepresentation(BsonType.String)]
            public IdentityType IdentityType { get; set; }
        }

        private enum IdentityType
        {
            SomeType = 42
        }
    }
}
