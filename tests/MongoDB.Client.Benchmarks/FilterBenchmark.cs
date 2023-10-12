using BenchmarkDotNet.Attributes;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Filters;

namespace MongoDB.Client.Benchmarks
{
    [BsonSerializable]
    public partial record BenchmarkModel(int SomeId);

    [MemoryDiagnoser]
    public class FilterBenchmark
    {
        [Benchmark]
        public void SimpleAndManual()
        {
            var andFilter = new AggregateFilter(AggregateFilterType.And);
            andFilter.Add(new EqFilter<int>("SomeId", 1));
            andFilter.Add(new EqFilter<int>("SomeId", 2));
            andFilter.Add(new EqFilter<int>("SomeId", 3));
            andFilter.Add(new EqFilter<int>("SomeId", 4));
        }
        [Benchmark]
        public void SimpleAndExpression()
        {
            _ = Filter.FromExpression((BenchmarkModel x) => x.SomeId == 1 && x.SomeId == 2 && x.SomeId == 3 && x.SomeId == 4);
        }
    }
}
