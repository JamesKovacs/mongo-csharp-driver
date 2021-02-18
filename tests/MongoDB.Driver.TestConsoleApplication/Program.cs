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
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace MongoDB.Driver.TestConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient();
            var db = client.GetDatabase("test");
            var coll = db.GetCollection<Person>("people");

            var query = from person in coll.AsQueryable()
                        where person.Age.Equals(42)
                        select person;
            foreach (var person in query.ToList())
            {
                Console.WriteLine(person);
            }
        }
    }

    class Person
    {
        public ObjectId Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }

        public override string ToString() => $"{GivenName} {Surname}";
    }
}
