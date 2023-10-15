namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class BsonElementAttribute : Attribute
    {
        public string ElementName { get; }
        public BsonElementAttribute(string elementName)
        {
            ElementName = elementName;
        }
    }
}
