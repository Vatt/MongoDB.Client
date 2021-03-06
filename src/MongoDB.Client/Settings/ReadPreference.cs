using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Settings
{
    public enum ReadPreference
    {
        [BsonElement("primary")]
        Primary,
        [BsonElement("primaryPreferred")]
        PrimaryPreferred,
        [BsonElement("secondary")]
        Secondary,
        [BsonElement("secondaryPreferred")]
        SecondaryPreferred,
        [BsonElement("nearest")]
        Nearest
    }
}
