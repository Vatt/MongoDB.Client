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
////    [BsonSerializable]
//    public record RecordWithConstructor : RecordModel
//    {
//        public int AA;
//        public long BB;
//        public double CC;
//        public string DD;
//        public Guid EE;
//        [BsonConstructor]
//        public RecordWithConstructor(int AA, long BB, double CC, string DD, Guid EE) : base(42, 42, 42, "42", Guid.NewGuid())
//        {
//            this.AA = AA;
//            this.BB = BB;
//            this.CC = CC;
//            this.DD = DD;
//            this.EE = EE;
//        }
//    }
//    [BsonSerializable]
//    public record RecordWithBase : RecordWithConstructor
//    {
//        [BsonConstructor]
//        public RecordWithBase(BsonDocument Document) : base(42, 42,42,"42", Guid.NewGuid())
//        {
//            this.Document = Document;
//        }
//    }
}