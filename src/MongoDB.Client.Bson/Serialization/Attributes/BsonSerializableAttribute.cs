using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    public enum GeneratorMode
    {
        Default = 1,
        SwitchOperations = 2,
        ContextTree = 3
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
