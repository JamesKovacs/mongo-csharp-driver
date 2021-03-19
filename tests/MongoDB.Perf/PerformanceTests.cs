using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Perf
{
    public static class PerformanceTests
    {
        [BsonDiscriminator("Simple")]
        public class Simplest
        {
            public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        }

        public static async Task TestSimplestBsonDeserialization(int threadsCount)
        {
            Console.WriteLine($"Deserializing with {threadsCount} threads");

            var obj = new Simplest();
            var bson = obj.ToBson();

            Task RunTest(int index) => Task.Run(() =>
            {
                var iterNum = 1_000_000;

                Simplest rehydrated = null;

                for (int i = 0; i < iterNum; i++)
                {
                    rehydrated = BsonSerializer.Deserialize<Simplest>(bson);
                }

                // Console.WriteLine($"{index} Completed {rehydrated.Id}");
            });

            var start = Environment.TickCount;

            var tasks = Enumerable.Range(0, threadsCount).Select(e => RunTest(e)).ToArray();

            await Task.WhenAll(tasks);

            var end = Environment.TickCount;

            Console.WriteLine($"Elapsed time: {end - start} ms");
        }
    }
}
