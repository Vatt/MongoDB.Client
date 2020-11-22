using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class InsertResult : IParserResult
    {
        [BsonElement("n")]
        public int N { get; set; }
        
        [BsonElement("ok")]
        public double Ok { get; set; }

        [BsonElement("writeErrors")]
        public List<InsertError> WriteErrors { get; set; }
    }
    
    [BsonSerializable]
    public class InsertError
    {
        [BsonElement("index")]
        public int Index { get; set; }
        
        [BsonElement("code")]
        public int Code { get; set; }

        [BsonElement("errmsg")]
        public string ErrorMessage { get; set; }
    }
}