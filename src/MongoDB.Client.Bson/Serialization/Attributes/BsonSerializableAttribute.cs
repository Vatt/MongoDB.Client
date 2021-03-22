using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [Flags]
    public enum GeneratorMode : byte
    {
        IfConditions = 1,
        ConstuctorOnlyParameters = 2,  
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
