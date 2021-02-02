using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization.Models;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks
{
    public class ComplexBenchmarkRootDocument : ComplexBenchmarkBase<RootDocument>
    {
        [Benchmark]
        public async Task ComplexBenchmarkNewClient()
        {
            await Run();
        }
    }
}