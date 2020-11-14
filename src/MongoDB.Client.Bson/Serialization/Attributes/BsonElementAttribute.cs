using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class BsonElementAttribute : Attribute
    { 
        public BsonElementAttribute(string elementName)
        {

        }
    }
}
