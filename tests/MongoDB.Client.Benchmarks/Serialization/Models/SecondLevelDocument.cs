using System;
using System.Buffers;
using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonSerializable]
    public class SecondLevelDocument
    {
        public string TextField { get; set; }
        public float FloatField { get; set; }
        public List<ThirdLevelDocument> InnerDocuments { get; set; }
    }
}
