using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BsonSerializerExtAttribute : Attribute
    {
        public BsonSerializerExtAttribute(Type type)
        {

        }
    }
}
