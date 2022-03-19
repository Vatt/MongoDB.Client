using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class InsertResult : IParserResult
    {
        [BsonElement("n")]
        public int N { get; }

        [BsonElement("ok")]
        public double Ok { get; }

        [BsonElement("writeErrors")]
        public List<InsertError>? WriteErrors { get; }

        //[BsonElement("opTime")]
        //public OpTime OpTime { get; }

        [BsonElement("electionId")]
        public BsonObjectId ElectionId { get; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime ClusterTime { get; }

        [BsonElement("operationTime")]
        public BsonTimestamp OperationTime { get; }

        public InsertResult(int N, double Ok, List<InsertError>? WriteErrors, /*OpTime OpTime,*/ BsonObjectId ElectionId, MongoClusterTime ClusterTime, BsonTimestamp OperationTime)
        {
            this.N = N;
            this.Ok = Ok;
            this.WriteErrors = WriteErrors;
            //this.OpTime = OpTime;
            this.ElectionId = ElectionId;
            this.ClusterTime = ClusterTime;
            this.OperationTime = OperationTime;
        }
    }

    [BsonSerializable]
    public partial class InsertError
    {
        [BsonElement("index")]
        public int Index { get; }

        [BsonElement("code")]
        public int Code { get; }

        [BsonElement("errmsg")]
        public string ErrorMessage { get; }
        public InsertError(int Index, int Code, string ErrorMessage)
        {
            this.Index = Index;
            this.Code = Code;
            this.ErrorMessage = ErrorMessage;
        }
    }

    [BsonSerializable]
    public partial class OpTime
    {
        [BsonElement("ts")]
        public BsonTimestamp Timestamp { get; }

        [BsonElement("t")]
        public long T { get; }

        public OpTime(BsonTimestamp Timestamp, long T)
        {
            this.Timestamp = Timestamp;
            this.T = T;
        }
    }
}
