using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BsonSerializerAttribute : Attribute
    {
        public BsonSerializerAttribute(Type type)
        {

        }
    }
}
