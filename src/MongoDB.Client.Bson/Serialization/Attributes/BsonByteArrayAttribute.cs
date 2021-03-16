using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    public enum ByteArrayRepresentation
    {
        Generic = 1,
        MD5 = 2,
    }
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonByteArrayAttribute : Attribute
    {
        public BsonByteArrayAttribute()
        {

        }
        public BsonByteArrayAttribute(ByteArrayRepresentation representation)
        {

        }
    }
}
