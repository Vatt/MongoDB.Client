using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class ModelWithIgnore
    {
        public string Field { get; set; }

        [BsonIgnore]
        public string IgnoredField { get; set; }
    }
}
