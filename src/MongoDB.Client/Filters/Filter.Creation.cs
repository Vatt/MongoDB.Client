using System.Linq.Expressions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Expressions;

namespace MongoDB.Client.Filters
{
    public abstract partial class Filter : IBsonSerializer<Filter>
    {
        private static Filter Create(string propertyName, object? value, RangeFilterType type)
        {
            if (propertyName is null)
            {
                return ThrowHelper.Expression<Filter>($"{nameof(propertyName)} is null");
            }

            if (value is null)
            {
                return new RangeFilter<object>(propertyName, new List<object?>() { null }, type);
            }
            switch (value)
            {
                case string[] str: return new RangeFilter<string>(propertyName, str, type);
                case int[] int32: return new RangeFilter<int>(propertyName, int32, type);
                case long[] int64: return new RangeFilter<long>(propertyName, int64, type);
                case double[] doubleValue: return new RangeFilter<double>(propertyName, doubleValue, type);
                case decimal[] decimalValue: return new RangeFilter<decimal>(propertyName, decimalValue, type);
                case BsonObjectId[] objectId: return new RangeFilter<BsonObjectId>(propertyName, objectId, type);
                case BsonTimestamp[] timestamp: return new RangeFilter<BsonTimestamp>(propertyName, timestamp, type);
                case DateTimeOffset[] dt: return new RangeFilter<DateTimeOffset>(propertyName, dt, type);
                case Guid[] guid: return new RangeFilter<Guid>(propertyName, guid, type);
                case BsonObjectId?[] objectId: return new RangeFilter<BsonObjectId?>(propertyName, objectId, type);
                case BsonTimestamp?[] timestamp: return new RangeFilter<BsonTimestamp?>(propertyName, timestamp, type);
                case DateTimeOffset?[] dt: return new RangeFilter<DateTimeOffset?>(propertyName, dt, type);
                case Guid?[] guid: return new RangeFilter<Guid?>(propertyName, guid, type);
                case BsonDocument[] document: return new RangeFilter<BsonDocument>(propertyName, document, type);


                case IEnumerable<string> str: return new RangeFilter<string>(propertyName, str, type);
                case IEnumerable<int> int32: return new RangeFilter<int>(propertyName, int32, type);
                case IEnumerable<long> int64: return new RangeFilter<long>(propertyName, int64, type);
                case IEnumerable<double> doubleValue: return new RangeFilter<double>(propertyName, doubleValue, type);
                case IEnumerable<decimal> decimalValue: return new RangeFilter<decimal>(propertyName, decimalValue, type);
                case IEnumerable<BsonObjectId> objectId: return new RangeFilter<BsonObjectId>(propertyName, objectId, type);
                case IEnumerable<BsonTimestamp> timestamp: return new RangeFilter<BsonTimestamp>(propertyName, timestamp, type);
                case IEnumerable<DateTimeOffset> dt: return new RangeFilter<DateTimeOffset>(propertyName, dt, type);
                case IEnumerable<Guid> guid: return new RangeFilter<Guid>(propertyName, guid, type);
                case IEnumerable<BsonObjectId?> objectId: return new RangeFilter<BsonObjectId?>(propertyName, objectId, type);
                case IEnumerable<BsonTimestamp?> timestamp: return new RangeFilter<BsonTimestamp?>(propertyName, timestamp, type);
                case IEnumerable<DateTimeOffset?> dt: return new RangeFilter<DateTimeOffset?>(propertyName, dt, type);
                case IEnumerable<Guid?> guid: return new RangeFilter<Guid?>(propertyName, guid, type);
                case IEnumerable<BsonDocument> document: return new RangeFilter<BsonDocument>(propertyName, document, type);

            }

            return ThrowHelper.Expression<Filter>($"Unsupported type in RangeFilter - {value.GetType()}");
        }
        private static Filter Create(string propertyName, object? value, FilterType op)
        {
            if (propertyName is null)
            {
                return ThrowHelper.Expression<Filter>($"{nameof(propertyName)} is null");
            }

            if (value is null)
            {
                return new Filter<object>(propertyName, null, op);
            }
            switch (value)
            {
                case string str: return new Filter<string>(propertyName, str, op);
                case int int32: return new Filter<int>(propertyName, int32, op);
                case long int64: return new Filter<long>(propertyName, int64, op);
                case double doubleValue: return new Filter<double>(propertyName, doubleValue, op);
                case decimal decimalValue: return new Filter<decimal>(propertyName, decimalValue, op);
                case BsonObjectId objectId: return new Filter<BsonObjectId>(propertyName, objectId, op);
                case BsonTimestamp timestamp: return new Filter<BsonTimestamp>(propertyName, timestamp, op);
                case DateTimeOffset dt: return new Filter<DateTimeOffset>(propertyName, dt, op);
                case Guid guid: return new Filter<Guid>(propertyName, guid, op);
            }

            return ThrowHelper.Expression<Filter>($"Unsupported type in Filter - {value.GetType()}");
        }
        private static Filter Create(string propertyName, object? value)
        {
            if (propertyName is null)
            {
                return ThrowHelper.Expression<Filter>($"{nameof(propertyName)} is null");
            }

            if (value is null)
            {
                return new Filter<object>(propertyName, null, FilterType.Eq);
            }
            switch (value)
            {
                case string str: return new Filter<string>(propertyName, str, FilterType.Eq);
                case int int32: return new Filter<int>(propertyName, int32, FilterType.Eq);
                case long int64: return new Filter<long>(propertyName, int64, FilterType.Eq);
                case double doubleValue: return new Filter<double>(propertyName, doubleValue, FilterType.Eq);
                case decimal decimalValue: return new Filter<decimal>(propertyName, decimalValue, FilterType.Eq);
                case BsonObjectId objectId: return new Filter<BsonObjectId>(propertyName, objectId, FilterType.Eq);
                case BsonTimestamp timestamp: return new Filter<BsonTimestamp>(propertyName, timestamp, FilterType.Eq);
                case DateTimeOffset dt: return new Filter<DateTimeOffset>(propertyName, dt, FilterType.Eq);
                case Guid guid: return new Filter<Guid>(propertyName, guid, FilterType.Eq);
                case BsonDocument document: return FromDocument(document);
            }

            return ThrowHelper.Expression<Filter>($"Unsupported type in Filter<T> - {value.GetType()}");
        }

        //TODO: check generic type
        public static RangeFilter<T> In<T>(string name, params T[] values) => new RangeFilter<T>(name, values, RangeFilterType.In);
        public static RangeFilter<T> NotIn<T>(string name, params T[] values) => new RangeFilter<T>(name, values, RangeFilterType.NotIn);
        public static Filter<T> Eq<T>(string name, T value) => new Filter<T>(name, value, FilterType.Eq);
        public static Filter<T> Lt<T>(string name, T value) => new Filter<T>(name, value, FilterType.Lt);
        public static Filter<T> Lte<T>(string name, T value) => new Filter<T>(name, value, FilterType.Lte);
        public static Filter<T> Gt<T>(string name, T value) => new Filter<T>(name, value, FilterType.Gt);
        public static Filter<T> Gte<T>(string name, T value) => new Filter<T>(name, value, FilterType.Gte);
        public static Filter<T> Ne<T>(string name, T value) => new Filter<T>(name, value, FilterType.Ne);
    }
}
