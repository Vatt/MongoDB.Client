using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class ConcurrentRequestsManyItemsBench
    {
        private MongoCollection<GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        private const int RequestsCount = 64;
        [Params(1, 4, 8, 16, 32)] public int Parallelism { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = "ConcurrentManyItemsBench";
            var itemsCount = 100;

            var client = new MongoClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<GeoIp>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<GeoIp>(collectionName);


            for (int i = 0; i < itemsCount; i++)
            {
                var item = new GeoIp
                {
                    city = "St Petersburg",
                    country = "Russia",
                    countryCode = "RU",
                    isp = "NevalinkRoute",
                    lat = 59.8944f,
                    lon = 30.2642f,
                    org = "Nevalink Ltd.",
                    query = "31.134.191.87",
                    region = "SPE",
                    regionName = "St.-Petersburg",
                    status = "success",
                    timezone = "Europe/Moscow",
                    zip = 190000
                };
                _oldCollection.InsertOne(item);
            }
        }

        [GlobalCleanup]
        public void Clean()
        {
            _oldCollection.DeleteMany(FilterDefinition<GeoIp>.Empty);
        }

        private static readonly BsonDocument EmptyFilter = new BsonDocument();

        [Benchmark]
        public async Task NewClientToList()
        {
            var iterations = RequestsCount / Parallelism;
            var tasks = new Task<List<GeoIp>>[Parallelism];
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < Parallelism; j++)
                {
                    tasks[j] = _collection.Find(EmptyFilter).ToListAsync().AsTask();
                }
                var result = await Task.WhenAll(tasks);
            }
        }

        [Benchmark]
        public async Task OldClientToList()
        {
            var iterations = RequestsCount / Parallelism;
            var tasks = new Task<List<GeoIp>>[Parallelism];
            for (int i = 0; i < iterations; i++)
            {
                for (int j = 0; j < Parallelism; j++)
                {
                    tasks[j] = _oldCollection.Find(FilterDefinition<GeoIp>.Empty).ToListAsync();
                }
                var result = await Task.WhenAll(tasks);
            }
        }
    }
}