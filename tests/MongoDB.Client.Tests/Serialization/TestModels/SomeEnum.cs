using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonEnumSerializable(EnumRepresentation.Int32)]
    public enum SomeEnum
    {
        EnumValueOne = 0,
        EnumValueTwo = 1,
        EnumValueThree = 2
    }
}
