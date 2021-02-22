using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class CreateCollectionHeader
    {
        [BsonElement("create")]
        public string? CollectionName { get; set; }

        [BsonElement("$db")]
        public string? Db { get; set; }

        [BsonElement("lsid")]
        public SessionId? Lsid { get; set; }
    }
}