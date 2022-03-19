namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BsonIgnoreAttribute : Attribute
    {
        public BsonIgnoreAttribute() { }
    }
}
