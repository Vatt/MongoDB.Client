using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Filters;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial class DeleteBody
    {
        [BsonElement("q")]
        public Filter Filter { get; }

        [BsonElement("limit")]
        public int Limit { get; }
        public DeleteBody(Filter Filter, int Limit)
        {
            this.Filter = Filter;
            this.Limit = Limit;
        }

        //TODO: Collation object
        //[BsonElementField(ElementName = "collation")]
        //[BsonWriteIgnoreIf("Collation is null")]
        //public string Collation { get; set; }

        //[BsonElementField(ElementName = "hint")]
        //[BsonWriteIgnoreIf("Hint.IsEmpty")]
        //public BsonElement Hint { get; set; }
    }
}
