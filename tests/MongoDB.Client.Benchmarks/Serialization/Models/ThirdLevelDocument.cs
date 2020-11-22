using System;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonSerializable]
    public class ThirdLevelDocument
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }
    }
}
