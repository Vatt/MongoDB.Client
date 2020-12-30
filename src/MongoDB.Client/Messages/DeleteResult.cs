using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class DeleteResult : IParserResult
    {
        [BsonElement("n")]
        public int N { get; set; }

        [BsonElement("ok")]
        public double Ok { get; set; }
    }
}