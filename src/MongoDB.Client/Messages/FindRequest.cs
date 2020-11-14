using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class FindRequest
    {
        [BsonElement(ElementName = "find")]
        [BsonWriteIgnoreIf("Find is null")]
        public string Find { get; set; }

        [BsonElement(ElementName = "filter")]
        [BsonWriteIgnoreIf("Filter is null")]
        public BsonDocument Filter { get; set; }

        [BsonElement(ElementName = "limit")]
        [BsonWriteIgnoreIf("Limit < 1")]
        public int Limit { get; set; }

        
        
        [BsonElement(ElementName = "getMore")]
        [BsonWriteIgnoreIf("GetMore < 1")]
        public long GetMore { get; set; }
        
        [BsonElement(ElementName = "collection")]
        [BsonWriteIgnoreIf("Collection is null")]
        public string Collection { get; set; }
        
        
        
        [BsonElement(ElementName = "$db")]
        public string Db { get; set; }

        [BsonElement(ElementName = "lsid")]
        public SessionId Lsid { get; set; }
    }
}
