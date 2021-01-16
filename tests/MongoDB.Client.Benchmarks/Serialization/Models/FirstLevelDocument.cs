using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonSerializable]
    public partial class FirstLevelDocument
    {
        public string TextField { get; set; }

        public int IntField { get; set; }
        public List<SecondLevelDocument> InnerDocuments { get; set; }
    }
}
