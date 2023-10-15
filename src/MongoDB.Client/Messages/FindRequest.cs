using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Filters;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial class FindRequest
    {
        [BsonElement("find")]
        [BsonWriteIgnoreIf("Find is null")]
        public string? Find { get; }

        [BsonElement("filter")]
        [BsonWriteIgnoreIf("Filter is null")]
        public Filter? Filter { get; }

        [BsonElement("limit")]
        [BsonWriteIgnoreIf("Limit < 1")]
        public int Limit { get; }



        [BsonElement("getMore")]
        [BsonWriteIgnoreIf("GetMore < 1")]
        public long GetMore { get; }

        [BsonElement("collection")]
        [BsonWriteIgnoreIf("Collection is null")]
        public string? Collection { get; }



        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("$readPreference")]
        [BsonWriteIgnoreIf("ReadPreference is null")]
        public ReadPreference? ReadPreference { get; set; }

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
        public FindRequest(string? Find, Filter? Filter, int Limit, long GetMore, string? Collection, string Db, SessionId Lsid, MongoClusterTime? ClusterTime, long? TxnNumber, bool? StartTransaction, bool? Autocommit)
        {
            this.Find = Find;
            this.Filter = Filter;
            this.Limit = Limit;
            this.GetMore = GetMore;
            this.Collection = Collection;
            this.Db = Db;
            this.Lsid = Lsid;
            this.ClusterTime = ClusterTime;
            this.TxnNumber = TxnNumber;
            this.StartTransaction = StartTransaction;
            this.Autocommit = Autocommit;
        }


        public FindRequest(string? Find, Filter? Filter, int Limit, long GetMore, string? Collection, string Db, SessionId Lsid, MongoClusterTime ClusterTime, long TxnNumber, bool Autocommit)
            : this(Find, Filter, Limit, GetMore, Collection, Db, Lsid, ClusterTime, TxnNumber, null, Autocommit)
        {
        }


        public FindRequest(string? Find, Filter? Filter, int Limit, long GetMore, string? Collection, string Db, SessionId Lsid)
            : this(Find, Filter, Limit, GetMore, Collection, Db, Lsid, null, null, null, null)
        {

        }

        public FindRequest(string? Find, Filter? Filter, int Limit, long GetMore, string? Collection, string Db, SessionId Lsid, long TxnNumber)
            : this(Find, Filter, Limit, GetMore, Collection, Db, Lsid, null, TxnNumber, null, null)
        {

        }
    }
}
