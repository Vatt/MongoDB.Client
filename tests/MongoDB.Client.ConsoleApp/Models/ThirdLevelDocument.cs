using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.ConsoleApp.Models
{
    [BsonSerializable]
    public class ThirdLevelDocument
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }
    }
}
