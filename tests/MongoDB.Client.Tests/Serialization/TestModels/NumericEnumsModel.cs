using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    
    
    [BsonEnumSerializable(EnumRepresentation.Int32)]
    public enum Int32Enum
    {
        EnumInt32Value1,
        EnumInt32Value2,
        EnumInt32Value3,
        EnumInt32Value4,
        EnumInt32Value5,
    }

    [BsonEnumSerializable(EnumRepresentation.Int64)]
    public enum Int64Enum
    {
        EnumInt64Value1,
        EnumInt64Value2,
        EnumInt64Value3,
        EnumInt64Value4,
        EnumInt64Value5,
    }
    [BsonSerializable]
    public class NumericEnumsModel
    {
        public string Name;
        public Int32Enum Int32EnumValue;
        public Int64Enum Int64EnumValue;
    }
}