using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class TransactionResult : IParserResult
    {
        [BsonElement("ok")]
        public double Ok { get; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime ClusterTime { get; }

        [BsonElement("operationTime")]
        public BsonTimestamp OperationTime { get; }

        [BsonElement("errorLabels")]
        public List<string>? ErrorLabels { get; }

        [BsonElement("errmsg")]
        public string? ErrorMessage { get; }

        [BsonElement("code")]
        public int? Code { get; }

        [BsonElement("codeName")]
        public string? CodeName { get; }

        [BsonConstructor]
        public TransactionResult(double Ok, MongoClusterTime ClusterTime, BsonTimestamp OperationTime, List<string>? ErrorLabels, string? ErrorMessage, int? Code, string? CodeName)
        {
            this.Ok = Ok;
            this.ClusterTime = ClusterTime;
            this.OperationTime = OperationTime;
            this.ErrorLabels = ErrorLabels;
            this.ErrorMessage = ErrorMessage;
            this.Code = Code;
            this.CodeName = CodeName;
        }
    }
}