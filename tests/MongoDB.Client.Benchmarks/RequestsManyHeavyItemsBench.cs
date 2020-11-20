using System;
using System.Net;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsManyHeavyItemsBench
    {
        private MongoCollection<RootDocument> _collection;
        private IMongoCollection<RootDocument> _oldCollection;

        [Params( 30000)]
        public int ItemsCount { get; set; }
        
        [GlobalSetup]
        public void Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = GetType().Name;
            
            var client = new MongoClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<RootDocument>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<RootDocument>(collectionName);

            _oldCollection.DeleteMany(FilterDefinition<RootDocument>.Empty);
            var gen = new DatabaseSeeder();
            var items = gen.GenerateSeed(ItemsCount);
            foreach (var item in items)
            {
                _oldCollection.InsertOne(item);
            }
        }

        [GlobalCleanup]
        public void Clean()
        {
            _oldCollection.DeleteMany(FilterDefinition<RootDocument>.Empty);
        }
        
        private static readonly BsonDocument EmptyFilter = new BsonDocument();
        
        [Benchmark]
        public async Task<int> NewClientToList()
        {
            var result = await _collection.Find(EmptyFilter).ToListAsync();
            return result.Count;
        }

        [Benchmark]
        public async Task<int> OldClientToList()
        {
            var result = await _oldCollection.Find(FilterDefinition<RootDocument>.Empty).ToListAsync();
            return result.Count;
        }
    }
}