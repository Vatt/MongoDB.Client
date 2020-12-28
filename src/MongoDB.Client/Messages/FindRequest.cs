using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class FindRequest
    {
        [BsonElement("find")]
        [BsonWriteIgnoreIf("Find is null")]
        public string Find { get; set; }

        [BsonElement("filter")]
        [BsonWriteIgnoreIf("Filter is null")]
        public BsonDocument Filter { get; set; }

        [BsonElement("limit")]
        [BsonWriteIgnoreIf("Limit < 1")]
        public int Limit { get; set; }

        
        
        [BsonElement("getMore")]
        [BsonWriteIgnoreIf("GetMore < 1")]
        public long GetMore { get; set; }
        
        [BsonElement("collection")]
        [BsonWriteIgnoreIf("Collection is null")]
        public string Collection { get; set; }
        
        
        
        [BsonElement("$db")]
        public string Db { get; set; }

        [BsonElement("lsid")]
        public SessionId Lsid { get; set; }
    }
}
