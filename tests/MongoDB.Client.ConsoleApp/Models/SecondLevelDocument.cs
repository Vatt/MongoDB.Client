using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.ConsoleApp.Models
{
    [BsonSerializable]
    public class SecondLevelDocument
    {
        public string TextField { get; set; }
        public int IntField { get; set; }
        public List<ThirdLevelDocument> InnerDocuments { get; set; }
    }
}
