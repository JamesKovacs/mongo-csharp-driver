using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace MongoDB.Perf
{
    public static class PerformanceTests
    {
        public class A
        {
            public string A1 { get; set; } = "a1aadsfsdafsdaf";
        }

        public class B : A
        {
            public string B1 { get; set; } = "b1adsfsdafsdaf";
        }

        public class C
        {
            public string S1 { get; set; } = "1adsfsdafsdaf";
            public string S2 { get; set; } = "2adsfsdafsdaf";
            public string S3 { get; set; } = "3adsfsdafsdaf";
            public string S4 { get; set; } = "4adsfsdafsdaf";
            public string S5 { get; set; } = "5adsfsdafsdaf";

            public int I1 { get; set; } = 1;
            public int I2 { get; set; } = 2;
            public int I3 { get; set; } = 3;
            public int I4 { get; set; } = 4;
            public int I5 { get; set; } = 5;

            public A AisB { get; set; } = new B();
        }
        public class Cat : Animal
        {
            public string Vaccinations { get; set; }
        }

        public class Dog : Animal
        {
            public string Vaccinations { get; set; }
        }

        [BsonDiscriminator(RootClass = true)]
        [BsonKnownTypes(typeof(Cat), typeof(Dog))]
        public class Animal
        {
            public string Name { get; set; }
            public string Info { get; set; }
            public DateTime DOB { get; set; }
        }

        public class Townhouse : REProperty
        {
            public string Notes { get; set; }
            public int Floors { get; set; }
        }

        public class Condo : REProperty
        {
            public string Notes { get; set; }
            public int FloorNum { get; set; }
        }

        [BsonDiscriminator(RootClass = true)]
        [BsonKnownTypes(typeof(Townhouse), typeof(Condo))]
        public class REProperty
        {
            public string Name { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class Person
        {
            public string Name { get; set; }
            public string LastName { get; set; }
            public string LastName2 { get; set; }
            public string LastName3 { get; set; }
            public string LastName4 { get; set; }
            public string LastName5 { get; set; }

            public Animal[] Pets { get; set; }
            public REProperty[] Properties { get; set; }
        }

        public static void Test_BsonWrite1()
        {
            var iterNum = 10000000;
            var fieldsNum = 20;
            var fieldsNames = Enumerable.Range(0, fieldsNum).Select(e => $"f_{e}").ToArray();

            var ms = Environment.TickCount;

            for (int i = 0; i < iterNum; i++)
            {
                var document = new BsonDocument();

                using (var writer = new BsonDocumentWriter(document))
                {
                    writer.WriteStartDocument();
                    for (int j = 0; j < 20; j++)
                    {
                        writer.WriteString(fieldsNames[j], "x");
                    }
                    writer.WriteEndDocument();
                }
            }

            ms = Environment.TickCount - ms;
            Console.WriteLine(ms);
        }

        public static async Task Test_BsonWrite2(int threadsCount)
        {
            Console.WriteLine($"Deserializing with {threadsCount} threads");

            var classMap = BsonClassMap.LookupClassMap(typeof(B));

            var obj = new C();
            var bson = obj.ToBson();

            Task RunTest(int index) => Task.Run(() =>
            {
                var iterNum = 1000000;

                C rehydrated = null;

                for (int i = 0; i < iterNum; i++)
                {
                    rehydrated = BsonSerializer.Deserialize<C>(bson);
                }

                Console.WriteLine($"{index} Completed {rehydrated.I1}");
            });

            var ms = Environment.TickCount;

            var tasks = Enumerable.Range(0, threadsCount).Select(e => RunTest(e)).ToArray();

            await Task.WhenAll(tasks);

            ms = Environment.TickCount - ms;

            Console.WriteLine(ms);
        }

        public static async Task Test_BsonDeserializeOnline(int threadsCount)
        {
            Console.WriteLine($"Deserializing with {threadsCount} threads");
            Console.WriteLine("Warming up");

            var client = new MongoClient("mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false");
            var db = client.GetDatabase("perf_testing");
            var collection = db.GetCollection<Person>("persons");

            // Warm up
            for (int i = 0; i < 3; i++)
            {
                var data = await collection.Find(new BsonDocument()).Skip(1000 * i).Limit(500).ToListAsync();
            }

            Console.WriteLine("Warm up complete");

            async Task RunTest(int index)
            {
                int itr = 2;
                var limit = 40000;

                List<Person> data = null;
                for (int i = 0; i < itr; i++)
                {
                    data = await collection.Find(new BsonDocument()).Limit(limit).ToListAsync();
                }

                Console.WriteLine($"Thread {index} completed with {data.Count} items, {itr} iterations");
            };

            var ms = Environment.TickCount;

            var tasks = Enumerable.Range(0, threadsCount).Select(e => RunTest(e)).ToArray();
            await Task.WhenAll(tasks);

            ms = Environment.TickCount - ms;

            Console.WriteLine($"\nTotal {ms}ms");
        }
    }
}
