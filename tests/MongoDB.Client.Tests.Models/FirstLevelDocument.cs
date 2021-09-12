using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class FirstLevelDocument
    {
        public string TextField { get; set; }

        public int IntField { get; set; }
        public List<SecondLevelDocument> InnerDocuments { get; set; }
    }
}
