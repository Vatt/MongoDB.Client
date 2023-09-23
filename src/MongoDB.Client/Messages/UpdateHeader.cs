using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class UpdateHeader
    {
        [BsonElement("update")]
        public string Update { get; }

        [BsonElement("ordered")]
        public bool Ordered { get; }

        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("$clusterTime")]
        [BsonWriteIgnoreIf("ClusterTime is null")]
        public MongoClusterTime? ClusterTime { get; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }

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
        public UpdateHeader(string update, bool ordered, string db, SessionId lsid, MongoClusterTime? clusterTime, long? txnNumber, bool? startTransaction, bool? autocommit)
        {

            Update = update;
            Ordered = ordered;
            Db = db;
            ClusterTime = clusterTime;
            Lsid = lsid;
            TxnNumber = txnNumber;
            StartTransaction = startTransaction;
            Autocommit = autocommit;
        }
        public UpdateHeader(string update, bool Ordered, string Db, SessionId Lsid, MongoClusterTime ClusterTime, long TxnNumber, bool Autocommit)
            : this(update, Ordered, Db, Lsid, ClusterTime, TxnNumber, null, Autocommit)
        {
        }


        public UpdateHeader(string update, bool Ordered, string Db, SessionId Lsid)
            : this(update, Ordered, Db, Lsid, null, null, null, null)
        {

        }

        public UpdateHeader(string update, bool Ordered, string Db, SessionId Lsid, long TxnNumber)
            : this(update, Ordered, Db, Lsid, null, TxnNumber, null, null)
        {

        }
    }
}
