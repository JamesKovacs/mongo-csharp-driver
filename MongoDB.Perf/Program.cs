using System.Threading.Tasks;

namespace MongoDB.Perf
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var threadCount = args?.Length > 0 ? int.Parse(args[0]) : 4;
            await PerformanceTests.Test_BsonWrite2(threadCount);
            //Console.ReadKey();
        }
    }
}
