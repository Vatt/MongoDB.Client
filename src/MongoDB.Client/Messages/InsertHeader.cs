using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class InsertHeader
    {
        [BsonElement("insert")]
        public string Insert { get; }

        [BsonElement("ordered")]
        public bool Ordered { get; }

        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }
        public InsertHeader(string Insert, bool Ordered, string Db, SessionId Lsid)
        {
            this.Insert = Insert;
            this.Ordered = Ordered;
            this.Db = Db;
            this.Lsid = Lsid;
        }
    }
}