using System.Collections.Generic;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    public class DeleteResult : IParserResult
    {
        public BsonDocument Document { get; set; }
    }
}