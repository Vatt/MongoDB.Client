using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class ThirdLevelDocument
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }
    }
}
