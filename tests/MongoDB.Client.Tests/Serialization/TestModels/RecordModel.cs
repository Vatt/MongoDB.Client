using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial record RecordModel0(int A, long B, double C, string D, Guid E)
    {
        public BsonDocument Document { get; set; }
        public void DoSome()
        {

        }
    }
}