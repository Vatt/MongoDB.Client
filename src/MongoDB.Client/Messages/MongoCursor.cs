using System.Runtime.CompilerServices;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Messages
{
    public unsafe static class CursorItemSerializer
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool TryParseBson<T>(ref BsonReader reader, out T item) where T : IBsonSerializer<T>
        {
            return T.TryParseBson(ref reader, out item);
        }
        public static void WriteBson<T>(ref BsonWriter writer, in T message, out byte bsonType) where T : IBsonSerializer<T>
        {
            throw new NotImplementedException(nameof(CursorItemSerializer));
        }
    }
    //[BsonSerializable]
    public partial class CursorResult<T> : IParserResult
    //where T : IBsonSerializer<T>
    {
        [BsonElement("cursor")]
        public MongoCursor<T> MongoCursor { get; set; }

        [BsonElement("ok")]
        public double Ok { get; set; }

        [BsonElement("errmsg")]
        public string? ErrorMessage { get; set; }

        [BsonElement("code")]
        public int Code { get; set; }

        [BsonElement("codeName")]
        public string? CodeName { get; set; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime? ClusterTime { get; set; }

        [BsonElement("operationTime")]
        public BsonTimestamp? OperationTime { get; set; }

        public CursorResult(MongoCursor<T> mongoCursor)
        {
            MongoCursor = mongoCursor;
        }
        public CursorResult(MongoCursor<T> mongoCursor, double ok, string? errorMessage, int code, string? codeName, MongoClusterTime? clusterTime, BsonTimestamp? operationTime)
        {
            MongoCursor = mongoCursor;
            Ok = ok;
            ErrorMessage = errorMessage;
            Code = code;
            CodeName = codeName;
            ClusterTime = clusterTime;
            OperationTime = operationTime;
        }
    }
    //[BsonSerializable]
    public partial class MongoCursor<T>
    {
        [BsonElement("id")]
        public long Id { get; set; }

        [BsonElement("ns")]
        public string? Namespace { get; set; }

        [BsonElement("firstBatch")]
        [BsonSerializer(typeof(CursorItemSerializer))]
        public List<T>? FirstBatch { get; set; }

        [BsonElement("nextBatch")]
        [BsonSerializer(typeof(CursorItemSerializer))]
        public List<T>? NextBatch { get; set; }
        public List<T>? Items { get; set; }
        public MongoCursor(List<T> items)
        {
            Items = items;
        }
        public MongoCursor(long id, string? _namespace, List<T> firstBatch, List<T> nextBatch)
        {
            Id = id;
            Namespace = _namespace;
            FirstBatch = firstBatch;
            NextBatch = nextBatch;
        }
    }
}
