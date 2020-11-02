using Microsoft.Extensions.Logging;

namespace MongoDB.Client
{
    public class MongoDatabase
    {
        private readonly ChannelsPool _channelsPool;
        private readonly ILoggerFactory _loggerFactory;
        public MongoClient Client { get; }
        public string Name { get; }

        internal MongoDatabase(MongoClient client, string name, ChannelsPool channelsPool, ILoggerFactory loggerFactory)
        {
            _channelsPool = channelsPool;
            _loggerFactory = loggerFactory;
            Client = client;
            Name = name;
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            return new MongoCollection<T>(this, name, _channelsPool, _loggerFactory);
        }
    }
}
