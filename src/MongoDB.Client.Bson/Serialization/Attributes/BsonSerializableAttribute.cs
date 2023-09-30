namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [Flags]
    public enum GeneratorMode : byte
    {
        IfConditions = 1,
        ConstructorParameters = 2,
        SkipTryParseBson = 4,
        SkipWriteBson = 8,
        DisableTypeChecks = 16,
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
