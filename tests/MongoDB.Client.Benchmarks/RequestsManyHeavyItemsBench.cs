using System.Net;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Tests.Models;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsManyHeavyItemsBench
    {
        private MongoCollection<RootDocument> _collection;
        private IMongoCollection<RootDocument> _oldCollection;

        [Params(1, 100, 500, 1000)]
        public int ItemsCount { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = GetType().Name;

            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<RootDocument>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<RootDocument>(collectionName);

            _oldCollection.DeleteMany(FilterDefinition<RootDocument>.Empty);
            var gen = new RootDocumentSeeder();
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
            var cursor = _collection.Find(EmptyFilter);
            var counter = 0;
            await foreach (var item in cursor)
            {
                counter += item.IntField;
            }

            return counter;
        }

        [Benchmark(Baseline = true)]
        public async Task<int> OldClientToList()
        {
            var source = await _oldCollection.FindAsync(FilterDefinition<RootDocument>.Empty);
            var counter = 0;
            using (source)
            {
                while (true)
                {
                    if (await source.MoveNextAsync(default).ConfigureAwait(false))
                    {
                        foreach (var item in source.Current)
                        {
                            counter += item.IntField;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return counter;
        }
    }
}
