using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    [BsonSerializable]
    public class TopologyVersion
    {
        [BsonElementField(ElementName = "processId")]
        public BsonObjectId ProcesssId { get; set; }

        [BsonElementField(ElementName = "counter")]
        public long Counter { get; set; }
    }
    [BsonSerializable]
    public class MongoDBConnectionInfo
    {
        [BsonElementField(ElementName = "ismaster")]
        public bool IsMaster { get; set; }

        [BsonElementField(ElementName = "topologyVersion")]
        public TopologyVersion Topology { get; set; }
        
        [BsonElementField(ElementName = "maxBsonObjectSize")]
        public int MaxBsonObjectSize { get; set; }

        [BsonElementField(ElementName = "maxMessageSizeBytes")]
        public int MaxMessageSizeBytes { get; set; }

        [BsonElementField(ElementName = "maxWriteBatchSize")]
        public int MaxWriteBatchSize { get; set; }

        [BsonElementField(ElementName = "localTime")]
        public DateTimeOffset LocalTime { get; set; }

        [BsonElementField(ElementName = "logicalSessionTimeoutMinutes")]
        public int LogicalSessionTimeoutMinutes { get; set; }

        [BsonElementField(ElementName = "connectionId")]
        public int ConnectionId { get; set; }

        [BsonElementField(ElementName = "minWireVersion")]
        public int MinWireVersion { get; set; }

        [BsonElementField(ElementName = "maxWireVersion")]
        public int MaxWireVersion { get; set; }

        [BsonElementField(ElementName = "readOnly")]
        public bool IsReadOnly { get; set; }

        [BsonElementField(ElementName = "ok")]
        public double Ok { get; set; }

    }
}
