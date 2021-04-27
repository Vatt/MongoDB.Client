using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class CursorResult<T> : IParserResult
    {
        [BsonElement("cursor")]
        public MongoCursor<T> MongoCursor { get; set; }

        public CursorResult(MongoCursor<T> mongoCursor)
        {
            MongoCursor = mongoCursor;
        }
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
    }
    [BsonSerializable]
    public partial class MongoCursor<T>
    {
        [BsonElement("id")]
        public long Id { get; set; }
        
        [BsonElement("ns")]
        public string? Namespace { get; set; }
        
        [BsonIgnore]
        public List<T> Items { get; set; }

        [BsonElement("firstBatch")]
        public List<T>? FirstBatch { get; set; }

        [BsonElement("nextBatch")]
        public List<T>? NextBatch { get; set; }

        public MongoCursor()
        {

        }
        public MongoCursor(List<T> items)
        {
            Items = items;
        }
    }
}
