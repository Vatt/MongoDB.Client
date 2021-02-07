using BenchmarkDotNet.Attributes;
using MongoDB.Client.Tests.Models;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks
{
    public class ComplexBenchmarkRootDocument : ComplexBenchmarkBase<RootDocument>
    {

    }
}