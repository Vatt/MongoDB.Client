using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class CreateCollectionResult : IParserResult
    {
        [BsonElement("nIndexesWas")]
        public int NIndexesWas { get; set; }

        [BsonElement("ns")]
        public string? Namespace { get; set; }

        [BsonElement("ok")]
        public double Ok { get; set; }

        [BsonElement("errmsg")]
        public string? ErrorMessage { get; set; }

        [BsonElement("code")]
        public int Code { get; set; }

        [BsonElement("codeName")]
        public string? CodeName { get; set; }
    }
}
