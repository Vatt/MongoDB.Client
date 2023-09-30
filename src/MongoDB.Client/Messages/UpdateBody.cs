using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial class UpdateBody
    {
        [BsonElement("q")]
        [BsonWriteIgnoreIf("Filter is null")]
        public BsonDocument Filter { get; }

        [BsonElement("u")]
        [BsonWriteIgnoreIf("Update is null")]
        public Update Update { get; }

        [BsonElement("multi")]
        public bool IsMulty { get; }

        [BsonElement("upsert")]
        public bool IsUpsert { get; }

        [BsonElement("collation")]
        public Collation? Collation { get; }

        [BsonElement("arrayFilters")]
        [BsonWriteIgnoreIf("ArrayFilters is null")]
        public List<BsonDocument>? ArrayFilters { get; }
        /*
                [BsonElement("hint")]
                public BsonElement? Hint { get; }
                */
        public UpdateBody(BsonDocument filter, Update update, bool isMulty, bool isUpsert = false, List<BsonDocument>? arrayFilters = null, Collation? collation = null)
        {
            Filter = filter;
            Update = update;
            IsMulty = isMulty;
            IsUpsert = isUpsert;
            Collation = collation;
            ArrayFilters = arrayFilters;
        }

    }
}
