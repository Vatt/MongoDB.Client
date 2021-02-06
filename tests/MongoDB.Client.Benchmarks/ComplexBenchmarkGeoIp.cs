using BenchmarkDotNet.Attributes;
using MongoDB.Client.Tests.Models;
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