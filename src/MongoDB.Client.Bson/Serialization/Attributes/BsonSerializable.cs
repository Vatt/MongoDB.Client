using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class BsonSerializable : Attribute
    {
        public BsonSerializable()
        {

        }
    }
}
