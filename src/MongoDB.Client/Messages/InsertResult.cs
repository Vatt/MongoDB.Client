using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class InsertResult : IParserResult
    {
        [BsonElement(ElementName = "n")]
        public int N { get; set; }
        
        [BsonElement(ElementName = "ok")]
        public double Ok { get; set; }

        [BsonElement(ElementName = "writeErrors")]
        public List<InsertError>? WriteErrors { get; set; }
    }
    
    [BsonSerializable]
    public class InsertError
    {
        [BsonElement(ElementName = "index")]
        public int Index { get; set; }
        
        [BsonElement(ElementName = "code")]
        public int Code { get; set; }

        [BsonElement(ElementName = "errmsg")]
        public string ErrorMessage { get; set; }
    }
}