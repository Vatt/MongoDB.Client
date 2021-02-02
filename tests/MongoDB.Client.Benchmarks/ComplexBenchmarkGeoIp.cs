using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization.Models;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks
{
    public class ComplexBenchmarkGeoIp : ComplexBenchmarkBase<GeoIp>
    {
        [Benchmark]
        public async Task ComplexBenchmarkNewClient()
        {
            await Run();
        }
    }
}