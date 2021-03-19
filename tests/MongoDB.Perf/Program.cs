using System;
using System.Threading.Tasks;

namespace MongoDB.Perf
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var threadCount = args?.Length > 0 ? int.Parse(args[0]) : Environment.ProcessorCount;
            await PerformanceTests.TestSimplestBsonDeserialization(threadCount);
        }
    }
}
