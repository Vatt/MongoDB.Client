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

        public DeleteHeader(string Delete, bool Ordered, string Db, SessionId Lsid)
        {
            this.Delete = Delete;
            this.Ordered = Ordered;
            this.Db = Db;
            this.Lsid = Lsid;
        }
    }
}