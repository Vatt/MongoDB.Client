namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class BsonElementAttribute : Attribute
    {
        public BsonElementAttribute(string elementName)
        {

        }
    }
}
