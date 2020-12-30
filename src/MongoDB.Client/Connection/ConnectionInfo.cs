using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Connection
{
    public class ConnectionInfo
    {
        public BsonDocument MongoConnectionInfo { get; }
        public BsonDocument Hell { get; }

        public ConnectionInfo(BsonDocument mongoConnectionInfo, BsonDocument hell)
        {
            MongoConnectionInfo = mongoConnectionInfo;
            Hell = hell;
        }
    }
}
