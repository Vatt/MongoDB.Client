using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.ConsoleApp.Models
{
    [BsonSerializable]
    public partial class ThirdLevelDocument
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }
    }
}
