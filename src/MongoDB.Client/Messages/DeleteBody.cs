using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class DeleteBody
    {
        [BsonElementField(ElementName = "q")]
        public BsonDocument Filter { get; set; }

        [BsonElementField(ElementName = "limit")]
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