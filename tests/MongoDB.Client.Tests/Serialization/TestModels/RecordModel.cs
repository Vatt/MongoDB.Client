using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public sealed record RecordModel(int A, long B, double C, string D, Guid E)
    {
        public BsonDocument Document { get; set; }
    }
    [BsonSerializable]
    public record RecordWithConstructor
    {
        public int A;
        public long B;
        public double C;
        public string D;
        public Guid E;
        public BsonDocument Document;
        [BsonConstructor]
        public RecordWithConstructor(int A, long B, double C, string D, Guid E)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
            this.E = E;
        }
    }
}