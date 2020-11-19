using System;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class SessionId
    {
        public SessionId(Guid id)
        {
            Id = id;
        }

        public SessionId()
        : this(Guid.NewGuid())
        {
        }
        
        [BsonElement("id")]
        public Guid Id { get; set; }
    }
}