using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class TransactionRequest
    {
        [BsonElement("commitTransaction")]
        [BsonWriteIgnoreIf("CommitTransaction is null")]
        public int? CommitTransaction { get; }

        [BsonElement("abortTransaction")]
        [BsonWriteIgnoreIf("AbortTransaction is null")]
        public int? AbortTransaction { get; }

        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime ClusterTime { get; }

        [BsonElement("txnNumber")]
        public long TxnNumber { get; }

        [BsonElement("autocommit")]
        public bool Autocommit { get; }

        [BsonConstructor]
        public TransactionRequest(int? CommitTransaction, int? AbortTransaction, string Db, SessionId Lsid, MongoClusterTime ClusterTime, long TxnNumber, bool Autocommit)
        {
            this.CommitTransaction = CommitTransaction;
            this.AbortTransaction = AbortTransaction;
            this.Db = Db;
            this.Lsid = Lsid;
            this.ClusterTime = ClusterTime;
            this.TxnNumber = TxnNumber;
            this.Autocommit = Autocommit;
        }
    }
}
