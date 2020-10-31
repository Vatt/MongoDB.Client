using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Connection
{
    public class ConnectionInfo
    {
        public MongoConnectionInfo? MongoConnectionInfo { get; }
        public BsonDocument? Hell { get; }

        public ConnectionInfo(MongoConnectionInfo? mongoConnectionInfo, BsonDocument? hell)
        {
            MongoConnectionInfo = mongoConnectionInfo;
            Hell = hell;
        }
    }
}
