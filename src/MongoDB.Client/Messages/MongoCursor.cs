using System;
using System.Collections.Generic;
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
        public static unsafe bool TryParseBson<T>(ref BsonReader reader, out T item)
        {
            return SerializerFnPtrProvider<T>.TryParseFnPtr(ref reader, out item);
        }
        public static void WriteBson<T>(ref BsonWriter writer, in T message, out byte bsonType)
        {
            throw new NotImplementedException(nameof(CursorItemSerializer));
        }
    }
    [BsonSerializable]
    public partial class CursorResult<T> : IParserResult
    {
        [BsonElement("cursor")]
        public MongoCursor<T> MongoCursor { get; }
        
        [BsonElement("ok")]
        public double Ok { get; }

        [BsonElement("errmsg")]
        public string? ErrorMessage { get; }
        
        [BsonElement("code")]
        public int Code { get; }
        
        [BsonElement("codeName")]
        public string? CodeName { get; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime? ClusterTime { get; }

        [BsonElement("operationTime")]
        public BsonTimestamp? OperationTime { get; }
        
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
    [BsonSerializable]
    public partial class MongoCursor<T>
    {
        [BsonElement("id")]
        public long Id { get; }
        
        [BsonElement("ns")]
        public string? Namespace { get; }

        [BsonElement("firstBatch")]
        [BsonSerializer(typeof(CursorItemSerializer))]
        public List<T>? FirstBatch { get; }

        [BsonElement("nextBatch")]
        [BsonSerializer(typeof(CursorItemSerializer))]
        public List<T>? NextBatch { get; }

        public MongoCursor(long id, string? _namespace, List<T> firstBatch, List<T> nextBatch)
        {
            Id = id;
            Namespace = _namespace;
            FirstBatch = firstBatch;
            NextBatch = nextBatch;
        }
    }
}
