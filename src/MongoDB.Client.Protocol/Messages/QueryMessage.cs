using MongoDB.Client.Bson.Document;

namespace MongoDB.Client
{
    public class QueryMessage
    {
        public QueryMessage(int requestNumber, string database, BsonDocument document)
        {
            RequestNumber = requestNumber;
            Database = database;
            Document = document;
        }

        public int RequestNumber { get; }
        public string Database { get; }

        public BsonDocument Document { get; }
    }
}
