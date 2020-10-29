using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class Sample
    {
        public long LongValue;
        public string StringValue;
        public int IntValue;
        public double DoubleValue;
        public DateTimeOffset DateTimeValue { get; set; }
        public BsonObjectId ObjectId;
        public bool BooleanValue;
    }
}
