using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

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
    }
}
