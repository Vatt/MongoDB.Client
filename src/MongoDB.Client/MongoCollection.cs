using MongoDB.Client.Bson.Document;

namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        private readonly ChannelsPool _channelsPool;

        internal MongoCollection(MongoDatabase database, string name, ChannelsPool channelsPool)
        {
            _channelsPool = channelsPool;
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
        }
        
        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }

        public Cursor<T> Find(BsonDocument filter)
        {
            return new Cursor<T>(_channelsPool, filter, Namespace);
        }
    }
}
