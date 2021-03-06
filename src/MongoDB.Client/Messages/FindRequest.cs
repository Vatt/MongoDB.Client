using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class FindRequest
    {
        [BsonElement("find")]
        [BsonWriteIgnoreIf("Find is null")]
        public string? Find { get; }

        [BsonElement("filter")]
        [BsonWriteIgnoreIf("Filter is null")]
        public BsonDocument? Filter { get; }

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
        [BsonWriteIgnoreIf("ReadPreference != null")]
        public ReadPreference? ReadPreference { get; set; }

        [BsonElement("$clusterTime")]
        [BsonWriteIgnoreIf("ClusterTime != null")]
        public MongoClusterTime ClusterTime { get; set; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }

        public FindRequest(string? Find, BsonDocument? Filter, int Limit, long GetMore, string? Collection, string Db, SessionId Lsid)
        {
            this.Find = Find;
            this.Filter = Filter;
            this.Limit = Limit;
            this.GetMore = GetMore;
            this.Collection = Collection;
            this.Db = Db;
            this.Lsid = Lsid;
        }
    }
}
