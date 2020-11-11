using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class InsertHeader
    {
        [BsonElementField(ElementName = "insert")]
        public string Insert { get; set; }

        [BsonElementField(ElementName = "ordered")]
        public bool Ordered { get; set; }

        [BsonElementField(ElementName = "$db")]
        public string Db { get; set; }

        [BsonElementField(ElementName = "lsid")]
        public SessionId Lsid { get; set; }
    }
}