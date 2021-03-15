using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    public enum GeneratorMode
    {
        Default = 1,
        IfConditions = 2,
        
    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class BsonSerializableAttribute : Attribute
    {
        public BsonSerializableAttribute()
        {

        }
        public BsonSerializableAttribute(GeneratorMode mode)
        {
            
        }
    }
}
