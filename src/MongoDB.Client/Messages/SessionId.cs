using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class SessionId
    {
        [BsonConstructor]
        public SessionId(Guid Id)
        {
            this.Id = Id;
        }

        public SessionId()
        : this(Guid.NewGuid())
        {
        }

        [BsonElement("id")]
        public Guid Id { get; }
    }
}