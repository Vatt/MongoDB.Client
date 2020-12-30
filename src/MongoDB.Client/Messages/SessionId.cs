using MongoDB.Client.Bson.Serialization.Attributes;
using System;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class SessionId
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