using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client
{
    [BsonSerializable]
    public class Data
    {
        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
