using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Connection
{
    public class ConnectionInfo
    {
        public BsonDocument IsMaster { get; }
        public BsonDocument BuildInfo { get; }

        public ConnectionInfo(BsonDocument isMaster, BsonDocument buildInfo)
        {
            IsMaster = isMaster;
            BuildInfo = buildInfo;
        }
    }
}
