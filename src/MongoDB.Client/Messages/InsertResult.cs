using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Messages
{
    public class InsertResult : IParserResult
    {
        public int N { get; set; }
        public double Ok { get; set; }

        public BsonArray? Error { get; set; }
    }
}