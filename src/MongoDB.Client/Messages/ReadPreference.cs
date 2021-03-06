using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial struct ReadPreference
    {
        [BsonElement("mode")]
        public string? Mode { get; }

        [BsonConstructor]
        public ReadPreference(string? Mode)
        {
            this.Mode = Mode;
        }
    }
}
