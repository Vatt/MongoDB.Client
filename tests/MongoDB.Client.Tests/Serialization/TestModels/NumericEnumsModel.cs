using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    
    
    
    public enum Int32Enum
    {
        EnumInt32Value1,
        EnumInt32Value2,
        EnumInt32Value3,
        EnumInt32Value4,
        EnumInt32Value5,
    }

    
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
        [BsonEnum(EnumRepresentation.Int32)]
        public Int32Enum Int32EnumValue;
        [BsonEnum(EnumRepresentation.Int64)]
        public Int64Enum Int64EnumValue;
        public override bool Equals(object? obj)
        {
            return obj is not null &&  obj is NumericEnumsModel other && Name.Equals(other.Name) && 
                   Int32EnumValue == other.Int32EnumValue && Int64EnumValue == other.Int64EnumValue;
        }
    }
}