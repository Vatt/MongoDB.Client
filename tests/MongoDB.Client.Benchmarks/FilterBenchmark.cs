using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.StackSources;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Filters;

namespace MongoDB.Client.Benchmarks
{
    [BsonSerializable]
    public partial record BenchmarkModel(BsonObjectId Id, int SomeId);

    class Wrapper
    {
        public WrappedArray WrappedArrayField = new();
        public WrappedArray WrappedArrayProperty { get; } = new();
        public WrappedInt32 WrappedInt32 { get; } = new();
    }
    class WrappedArray
    {
        public int[] FieldArray = new[] { 1, 2, 3, 4, 5 };
        public int[] PropertyArray { get; } = new[] { 5, 4, 3, 2, 1 };
    }
    class WrappedInt32
    {
        public int Value { get; } = 1;
    }

    [MemoryDiagnoser]
    public class FilterBenchmark
    {
        private static Wrapper wrapper { get; set; } = new();
        private static int[] arr = new int[] { 1, 2, 3 };
        private static BsonObjectId id1 = BsonObjectId.NewObjectId();
        private static BsonObjectId id2 = BsonObjectId.NewObjectId();
        private static BsonObjectId id3 = BsonObjectId.NewObjectId();
        private static bool boolVar = false;

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
        [Benchmark]
        public void HardManual()
        {
            var orFilter = new AggregateFilter(AggregateFilterType.Or);
            orFilter.Add(new EqFilter<int>("SomeId", 1));
            var andFilter = new AggregateFilter(AggregateFilterType.And);
            andFilter.Add(new EqFilter<BsonObjectId>("Id", id1));
            andFilter.Add(new EqFilter<BsonObjectId>("Id", id2));
            andFilter.Add(new EqFilter<BsonObjectId>("Id", id3));
            andFilter.Add(new RangeFilter<int>("Id", arr, RangeFilterType.In));
            andFilter.Add(new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Gte));
            andFilter.Add(new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Lte));
            andFilter.Add(new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Lt));
            andFilter.Add(new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Gt));
            andFilter.Add(new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Ne));
            andFilter.Add(new EqFilter<int>("SomeId", 1));
            orFilter.Add(andFilter);
            var orFilter1 = new AggregateFilter(AggregateFilterType.Or);
            andFilter.Add(orFilter1);
            orFilter1.Add(new RangeFilter<int>("Id", wrapper.WrappedArrayField.PropertyArray, RangeFilterType.In));
            orFilter1.Add(new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.NotIn));
            orFilter1.Add(new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.NotIn));
            orFilter1.Add(new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.NotIn));
            orFilter1.Add(new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.In));
            orFilter.Add(andFilter);
        }
        [Benchmark]
        public void HardExpression()
        {
            _ = Filter.FromExpression((BenchmarkModel x) => FilterBenchmark.wrapper.WrappedArrayProperty.PropertyArray.Contains(x.SomeId) == false ||
                                                            FilterBenchmark.wrapper.WrappedArrayProperty.FieldArray.Contains(x.SomeId) != true ||
                                                            FilterBenchmark.wrapper.WrappedArrayField.FieldArray.Contains(x.SomeId) != true ||
                                                            FilterBenchmark.wrapper.WrappedArrayField.PropertyArray.Contains(x.SomeId) != boolVar &&
                                                            x.SomeId != FilterBenchmark.wrapper.WrappedInt32.Value &&
                                                            FilterBenchmark.wrapper.WrappedInt32.Value == x.SomeId &&
                                                            FilterBenchmark.wrapper.WrappedInt32.Value != x.SomeId &&
                                                            FilterBenchmark.wrapper.WrappedInt32.Value > x.SomeId &&
                                                            FilterBenchmark.wrapper.WrappedInt32.Value < x.SomeId &&
                                                            FilterBenchmark.wrapper.WrappedInt32.Value <= x.SomeId &&
                                                            FilterBenchmark.wrapper.WrappedInt32.Value >= x.SomeId &&
                                                            FilterBenchmark.arr.Contains(x.SomeId) &&
                                                            x.Id == FilterBenchmark.id1 &&
                                                            FilterBenchmark.id2 == x.Id &&
                                                            FilterBenchmark.id3 == x.Id &&
                                                            1 == x.SomeId ||
                                                            x.SomeId == 1);
        }
    }
}
