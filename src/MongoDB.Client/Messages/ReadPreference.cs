using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial struct ReadPreference
    {
        [BsonElement("mode")]
        public Settings.ReadPreference Mode { get; }

        [BsonConstructor]
        public ReadPreference(Settings.ReadPreference Mode)
        {
            this.Mode = Mode;
        }
    }
}
