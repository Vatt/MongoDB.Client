using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class DeleteHeader
    {
        [BsonElement("delete")]
        public string Delete { get; }

        [BsonElement("ordered")]
        public bool Ordered { get; }

        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }

        [BsonElement("$clusterTime")]
        [BsonWriteIgnoreIf("ClusterTime is null")]
        public MongoClusterTime? ClusterTime { get; }

        [BsonElement("txnNumber")]
        [BsonWriteIgnoreIf("TxnNumber is null")]
        public long? TxnNumber { get; }

        [BsonElement("startTransaction")]
        [BsonWriteIgnoreIf("StartTransaction is null")]
        public bool? StartTransaction { get; }

        [BsonElement("autocommit")]
        [BsonWriteIgnoreIf("Autocommit is null")]
        public bool? Autocommit { get; }

        [BsonConstructor]
        public DeleteHeader(string Delete, bool Ordered, string Db, SessionId Lsid, MongoClusterTime? ClusterTime, long? TxnNumber, bool? StartTransaction, bool? Autocommit)
        {
            this.Delete = Delete;
            this.Ordered = Ordered;
            this.Db = Db;
            this.Lsid = Lsid;
            this.ClusterTime = ClusterTime;
            this.TxnNumber = TxnNumber;
            this.StartTransaction = StartTransaction;
            this.Autocommit = Autocommit;
        }

        public DeleteHeader(string Delete, bool Ordered, string Db, SessionId Lsid, MongoClusterTime ClusterTime, long TxnNumber, bool Autocommit)
            : this(Delete, Ordered, Db, Lsid, ClusterTime, TxnNumber, null, Autocommit)
        {
        }


        public DeleteHeader(string Delete, bool Ordered, string Db, SessionId Lsid)
        : this(Delete, Ordered, Db, Lsid, null, null, null, null)
        {

        }

        public DeleteHeader(string Delete, bool Ordered, string Db, SessionId Lsid, long TxnNumber)
            : this(Delete, Ordered, Db, Lsid, null, TxnNumber, null, null)
        {

        }
    }
}