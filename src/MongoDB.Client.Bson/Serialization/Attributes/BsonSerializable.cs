using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BsonSerializable : Attribute
    {
        public BsonSerializable()
        {
            
        }
    }
}
