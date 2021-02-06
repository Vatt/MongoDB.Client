using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using System;
using System.Net;
using System.Threading.Tasks;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;
using SimpleModel = MongoDB.Client.Tests.Models.SimpleModel;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class ConcurrentRequestsManySimpleItemsBench
    {
        private MongoCollection<SimpleModel> _collection;
        private IMongoCollection<SimpleModel> _oldCollection;

        [Params(256)]
        public int RequestsCount { get; set; }

        [Params(100)]
        public int ItemInDb { get; set; }

        [Params(1, 4, 8, 16, 32, 64, 128)] public int Parallelism { get; set; }



        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = GetType().Name;
            var client = new MongoClient(new DnsEndPoint(host, 27017));
            await client.InitAsync();
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<SimpleModel>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<SimpleModel>(collectionName);

            oldDb.DropCollection(collectionName);
            for (int i = 0; i < ItemInDb; i++)
            {
                var item = new SimpleModel
                {
                    Value = i
                };
                _oldCollection.InsertOne(item);
            }
        }

        [GlobalCleanup]
        public void Clean()
        {
            _oldCollection.Database.DropCollection(GetType().Name);
        }

        private static readonly BsonDocument EmptyFilter = new BsonDocument();

        [Benchmark]
        public async Task NewClientToList()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < RequestsCount; i++)
                {
                    await _collection.Find(EmptyFilter).FirstOrDefaultAsync();
                }
            }
            else
            {
                var iterations = RequestsCount / Parallelism;
                var tasks = new Task<SimpleModel>[Parallelism];
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < Parallelism; j++)
                    {
                        tasks[j] = _collection.Find(EmptyFilter).FirstOrDefaultAsync().AsTask();
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }

        [Benchmark]
        public async Task OldClientToList()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < RequestsCount; i++)
                {
                    await _oldCollection.Find(FilterDefinition<SimpleModel>.Empty).FirstOrDefaultAsync();
                }
            }
            else
            {
                var iterations = RequestsCount / Parallelism;
                var tasks = new Task<SimpleModel>[Parallelism];
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < Parallelism; j++)
                    {
                        tasks[j] = _oldCollection.Find(FilterDefinition<SimpleModel>.Empty).FirstOrDefaultAsync();
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }
    }
}