using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public record RecordModel(int A, long B, double C, string D, Guid E)
    {
        public BsonDocument Document { get; set; }
    }
    [BsonSerializable]
    public record RecordWithConstructor(int AA, long BB, double CC, string DD, Guid EE) : RecordModel(42, 42, 42, "42", Guid.NewGuid());

    [BsonSerializable]
    public record RecordWithBase : RecordWithConstructor
    {
        [BsonConstructor]
        public RecordWithBase(BsonDocument Document) : base(42, 42, 42, "42", Guid.NewGuid())
        {
            this.Document = Document;
        }
    }
}