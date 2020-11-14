using System;


namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BsonWriteIgnoreIfAttribute : Attribute
    {
        public BsonWriteIgnoreIfAttribute(string condition) { }
    }
}
