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
            _ = AggregateFilter.And(new EqFilter<int>("SomeId", 1),
                                    new EqFilter<int>("SomeId", 2),
                                    new EqFilter<int>("SomeId", 3),
                                    new EqFilter<int>("SomeId", 4));

        }
        [Benchmark]
        public void SimpleAndExpression()
        {
            _ = Filter.FromExpression((BenchmarkModel x) => x.SomeId == 1 && x.SomeId == 2 && x.SomeId == 3 && x.SomeId == 4);
        }
        [Benchmark]
        public void HardManual()
        {
            _ = AggregateFilter.Or(new EqFilter<int>("SomeId", 1),
                                   AggregateFilter.And(new EqFilter<BsonObjectId>("Id", id1),
                                                       new EqFilter<BsonObjectId>("Id", id2),
                                                       new EqFilter<BsonObjectId>("Id", id3),
                                                       new RangeFilter<int>("Id", arr, RangeFilterType.In),
                                                       new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Gte),
                                                       new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Lte),
                                                       new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Lt),
                                                       new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Gt),
                                                       new Filter<int>("SomeId", wrapper.WrappedInt32.Value, FilterType.Ne),
                                                       new EqFilter<int>("SomeId", 1)),
                                   AggregateFilter.Or(new RangeFilter<int>("Id", wrapper.WrappedArrayField.PropertyArray, RangeFilterType.In),
                                                      new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.NotIn),
                                                      new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.NotIn),
                                                      new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.NotIn),
                                                      new RangeFilter<int>("Id", wrapper.WrappedArrayField.FieldArray, RangeFilterType.In)));

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
