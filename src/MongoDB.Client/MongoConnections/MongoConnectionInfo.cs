﻿using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.MongoConnections
{
    [BsonSerializable]
    public class MongoTopologyVersion
    {
        [BsonElement("processId")]
        public BsonObjectId ProcesssId;

        [BsonElement("counter")]
        public long Counter { get; set; }
    }

    [BsonSerializable]
    public class MongoConnectionInfo
    {
        [BsonElement("ismaster")]
        public bool IsMaster { get; set; }

        [BsonElement("topologyVersion")]
        public MongoTopologyVersion Topology { get; set; }

        [BsonElement("maxBsonObjectSize")]
        public int MaxBsonObjectSize { get; set; }

        [BsonElement("maxMessageSizeBytes")]
        public int MaxMessageSizeBytes { get; set; }

        [BsonElement("maxWriteBatchSize")]
        public int MaxWriteBatchSize { get; set; }

        [BsonElement("localTime")]
        public DateTimeOffset LocalTime { get; set; }

        [BsonElement("logicalSessionTimeoutMinutes")]
        public int LogicalSessionTimeoutMinutes { get; set; }

        [BsonElement("connectionId")]
        public int ConnectionId { get; set; }

        [BsonElement("minWireVersion")]
        public int MinWireVersion { get; set; }

        [BsonElement("maxWireVersion")]
        public int MaxWireVersion { get; set; }

        [BsonElement("readOnly")]
        public bool IsReadOnly { get; set; }

        [BsonElement("ok")]
        public double Ok { get; set; }

    }
}
