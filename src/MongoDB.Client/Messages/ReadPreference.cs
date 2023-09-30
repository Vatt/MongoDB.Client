using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial struct ReadPreference
    {
        [BsonElement("mode")]
        [BsonEnum(EnumRepresentation.String)]
        public Settings.ReadPreference Mode { get; }

        [BsonConstructor]
        public ReadPreference(Settings.ReadPreference Mode)
        {
            this.Mode = Mode;
        }
    }
}
