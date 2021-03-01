using MongoDB.Client.Connection;
using MongoDB.Client.Scheduler;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoDatabase
    {
        private readonly StandaloneScheduler _scheduler;
        public MongoClient Client { get; }
        public string Name { get; } //TODO: byte[] mb?

        internal MongoDatabase(MongoClient client, string name, StandaloneScheduler scheduler)
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

        public ValueTask CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            var collection = GetCollection<object>(collectionName);
            return collection.CreateAsync(cancellationToken);
        }
    }
}
