using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.MongoConnections
{
    [BsonSerializable]
    public class MongoTopologyVersion
    {
        [BsonElement(ElementName = "processId")]
        public BsonObjectId ProcesssId;

        [BsonElement(ElementName = "counter")]
        public long Counter { get; set; }
    }

    [BsonSerializable]
    public class MongoConnectionInfo
    {
        [BsonElement(ElementName = "ismaster")]
        public bool IsMaster { get; set; }

        [BsonElement(ElementName = "topologyVersion")]
        public MongoTopologyVersion Topology { get; set; }

        [BsonElement(ElementName = "maxBsonObjectSize")]
        public int MaxBsonObjectSize { get; set; }

        [BsonElement(ElementName = "maxMessageSizeBytes")]
        public int MaxMessageSizeBytes { get; set; }

        [BsonElement(ElementName = "maxWriteBatchSize")]
        public int MaxWriteBatchSize { get; set; }

        [BsonElement(ElementName = "localTime")]
        public DateTimeOffset LocalTime { get; set; }

        [BsonElement(ElementName = "logicalSessionTimeoutMinutes")]
        public int LogicalSessionTimeoutMinutes { get; set; }

        [BsonElement(ElementName = "connectionId")]
        public int ConnectionId { get; set; }

        [BsonElement(ElementName = "minWireVersion")]
        public int MinWireVersion { get; set; }

        [BsonElement(ElementName = "maxWireVersion")]
        public int MaxWireVersion { get; set; }

        [BsonElement(ElementName = "readOnly")]
        public bool IsReadOnly { get; set; }

        [BsonElement(ElementName = "ok")]
        public double Ok { get; set; }

    }
}
