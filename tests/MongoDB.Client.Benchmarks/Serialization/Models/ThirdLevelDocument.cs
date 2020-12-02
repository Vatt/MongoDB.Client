using System;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    [BsonSerializable]
    public partial class ThirdLevelDocument
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }
    }
}
