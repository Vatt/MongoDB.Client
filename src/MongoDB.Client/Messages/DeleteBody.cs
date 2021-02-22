using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class DeleteBody
    {
        [BsonElement("q")]
        public BsonDocument? Filter { get; set; }

        [BsonElement("limit")]
        public int Limit { get; set; }


        //TODO: Collation object
        //[BsonElementField(ElementName = "collation")]
        //[BsonWriteIgnoreIf("Collation is null")]
        //public string Collation { get; set; }

        //[BsonElementField(ElementName = "hint")]
        //[BsonWriteIgnoreIf("Hint.IsEmpty")]
        //public BsonElement Hint { get; set; }
    }
}