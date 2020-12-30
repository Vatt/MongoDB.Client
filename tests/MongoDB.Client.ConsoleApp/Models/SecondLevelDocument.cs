using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.ConsoleApp.Models
{
    [BsonSerializable]
    public partial class SecondLevelDocument
    {
        public string TextField { get; set; }
        public int IntField { get; set; }
        public List<ThirdLevelDocument> InnerDocuments { get; set; }
    }
}
