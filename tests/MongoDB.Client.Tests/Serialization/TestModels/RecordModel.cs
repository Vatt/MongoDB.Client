using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    //[BsonSerializable]
    //public record RecordModel0(int A, long B, double C, string D, Guid E)
    //{
    //    public BsonDocument Document { get; set; }
    //    public void DoSome()
    //    {

    //    }
    //}
    //[BsonSerializable]
    //public record RecordModel1(int AA, long BB, double CC, string DD, Guid EE) : RecordModel0(AA, BB, CC, DD, EE)
    //{
    //    public new BsonDocument Document;
    //}

    //[BsonSerializable]
    //public record RecordModel2(int NewInt) : RecordModel1(42, 42, 42, "42", Guid.NewGuid());

    //[BsonSerializable]
    //public record RecordModel3(int AA, long BB, double CC, string DD, Guid EE, int F, long G) : RecordModel1(AA, BB, CC, DD, EE);

}