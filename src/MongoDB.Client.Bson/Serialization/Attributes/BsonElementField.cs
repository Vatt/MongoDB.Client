using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class BsonElementField : Attribute
    {
        public string? ElementName { get; set; }
        public BsonElementField()
        {
            
        }
    }
}
