using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    public enum BinaryDataRepresentation
    {
        Generic = 1,
        MD5 = 2,
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonBinaryDataAttribute : Attribute
    {
        public BsonBinaryDataAttribute()
        {

        }
        public BsonBinaryDataAttribute(BinaryDataRepresentation representation)
        {

        }
    }
}
