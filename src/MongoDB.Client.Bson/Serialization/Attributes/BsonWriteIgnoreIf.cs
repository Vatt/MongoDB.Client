using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BsonWriteIgnoreIf : Attribute
    {
        public BsonWriteIgnoreIf(string condition) { }
    }
}
