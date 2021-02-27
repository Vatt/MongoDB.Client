using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class CreateCollectionHeader
    {
        [BsonElement("create")]
        public string CollectionName { get; }

        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }

        public CreateCollectionHeader(string CollectionName, string Db, SessionId Lsid)
        {
            this.CollectionName = CollectionName;
            this.Db = Db;
            this.Lsid = Lsid;
        }
    }
}