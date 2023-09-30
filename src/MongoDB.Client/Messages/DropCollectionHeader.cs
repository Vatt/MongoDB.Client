using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial class DropCollectionHeader
    {
        [BsonElement("drop")]
        public string CollectionName { get; }

        [BsonElement("$db")]
        public string Db { get; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; }

        public DropCollectionHeader(string CollectionName, string Db, SessionId Lsid)
        {
            this.CollectionName = CollectionName;
            this.Db = Db;
            this.Lsid = Lsid;
        }
    }
}
