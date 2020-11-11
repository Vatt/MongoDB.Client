using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public class InsertResult : IParserResult
    {
        [BsonElementField(ElementName = "n")]
        public int N { get; set; }
        
        [BsonElementField(ElementName = "ok")]
        public double Ok { get; set; }

        [BsonElementField(ElementName = "writeErrors")]
        public List<InsertError>? WriteErrors { get; set; }
    }
    
    [BsonSerializable]
    public class InsertError
    {
        [BsonElementField(ElementName = "index")]
        public int Index { get; set; }
        
        [BsonElementField(ElementName = "code")]
        public int Code { get; set; }

        [BsonElementField(ElementName = "errmsg")]
        public string ErrorMessage { get; set; }
    }
}