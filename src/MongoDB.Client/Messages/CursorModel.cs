using System;
using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class Inner<T>
    {
        [BsonElement("firstBatch")]
        public List<T> FirstBatch { get; set; }
    }
    [BsonSerializable]
    public partial class CursorDTO<T>
    {
        [BsonElement("cursor")]
        public Inner<T> Cursor { get; set; }

        [BsonElement("ok")]
        public double Ok { get; set; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime? ClusterTime { get; set; }
        
        [BsonElement("timestamp")]
        public BsonTimestamp? Timestamp { get; set; }
    }
}
