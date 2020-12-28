using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization
{
    [BsonSerializable]
    public partial class SimpleModel
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [BsonIgnore]
        public MongoDB.Bson.ObjectId OldId { get; set; }

        [BsonElement("_id")]
        [MongoDB.Bson.Serialization.Attributes.BsonIgnore]
        public Bson.Document.BsonObjectId Id { get; set; }

        public int Value { get; set; }
    }
}
