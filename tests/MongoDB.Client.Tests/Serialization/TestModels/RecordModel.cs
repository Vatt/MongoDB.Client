using System;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public sealed record RecordModel(int A, long B, double C, string D, Guid E);
}