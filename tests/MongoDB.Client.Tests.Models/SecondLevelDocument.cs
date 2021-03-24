using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class SecondLevelDocument
    {
        public string TextField { get; set; }
        public int IntField { get; set; }
        public List<ThirdLevelDocument> InnerDocuments { get; set; }
    }
}
