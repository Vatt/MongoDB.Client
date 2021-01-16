using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Connection;

namespace MongoDB.Client
{
    public class MongoDatabase
    {
        private readonly RequestScheduler _scheduler;
        public MongoClient Client { get; }
        public string Name { get; } //TODO: byte[] mb?

        internal MongoDatabase(MongoClient client, string name, RequestScheduler scheduler)
        {
            _scheduler = scheduler;
            Client = client;
            Name = name;
        }

        public MongoCollection<T> GetCollection<T>(string name)
        {
            return new MongoCollection<T>(this, name, _scheduler);
        }

        public ValueTask DropCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            var collection = GetCollection<object>(collectionName);
            return collection.DropAsync(cancellationToken);
        }
    }
}
