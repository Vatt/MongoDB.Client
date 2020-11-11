using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class FindRequest
    {
        [BsonElementField(ElementName = "find")]
        [BsonWriteIgnoreIf("Find is null")]
        public string Find { get; set; }

        [BsonElementField(ElementName = "filter")]
        [BsonWriteIgnoreIf("Filter is null")]
        public BsonDocument Filter { get; set; }

        [BsonElementField(ElementName = "limit")]
        [BsonWriteIgnoreIf("Limit < 1")]
        public int Limit { get; set; }

        
        
        [BsonElementField(ElementName = "getMore")]
        [BsonWriteIgnoreIf("GetMore < 1")]
        public long GetMore { get; set; }
        
        [BsonElementField(ElementName = "collection")]
        [BsonWriteIgnoreIf("Collection is null")]
        public string Collection { get; set; }
        
        
        
        [BsonElementField(ElementName = "$db")]
        public string Db { get; set; }

        [BsonElementField(ElementName = "lsid")]
        public SessionId Lsid { get; set; }
    }
}
