using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    public enum EnumRepresentation
    {
        String = 1,
        Int32 = 2,
        Int64 = 3,
    }
    [AttributeUsage(AttributeTargets.Enum)]
    public class BsonEnumSerializable : Attribute
    {
        public BsonEnumSerializable(EnumRepresentation representation)
        {
            var enumValue = 1;
            if (enumValue == (int)representation)
            {

            }
        }
    }
}