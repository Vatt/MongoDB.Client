using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    public enum BinaryDataRepresentation
    {
        Generic = 0,
        MD5 = 5,
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
