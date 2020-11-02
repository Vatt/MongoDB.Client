using Microsoft.Extensions.Logging;

namespace MongoDB.Client
{
    public class MongoDatabase
    {
        private readonly ILoggerFactory _loggerFactory;
        public MongoClient Client { get; }
        public string Name { get; }

        internal MongoDatabase(MongoClient client, string name, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            Client = client;
            Name = name;
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            var collection = new MongoCollection<T>(this, name, _loggerFactory);
            collection.BeginConnection();
            return collection;
        }
    }
}
