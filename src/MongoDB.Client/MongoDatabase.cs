using Microsoft.Extensions.Logging;

namespace MongoDB.Client
{
    public class MongoDatabase
    {
        private readonly ILogger _logger;
        public MongoClient Client { get; }
        public string Name { get; }

        internal MongoDatabase(MongoClient client, string name, ILogger logger)
        {
            _logger = logger;
            Client = client;
            Name = name;
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            var collection = new MongoCollection<T>(this, name, _logger);
            collection.BeginConnection();
            return collection;
        }
    }
}
