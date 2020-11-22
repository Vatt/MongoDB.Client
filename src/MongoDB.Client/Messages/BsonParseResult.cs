using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Messages
{
    public class BsonParseResult : IParserResult
    {
        public BsonDocument Document { get; set; }
    }
}