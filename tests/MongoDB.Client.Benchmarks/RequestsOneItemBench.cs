using BenchmarkDotNet.Attributes;
using MongoDB.Client.Tests.Models;
using MongoDB.Driver;
using System;
using System.Net;
using System.Threading.Tasks;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsOneItemBench
    {
        private MongoCollection<GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = GetType().Name;

            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<GeoIp>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<GeoIp>(collectionName);

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

        [GlobalCleanup]
        public void Clean()
        {
            _oldCollection.DeleteMany(FilterDefinition<GeoIp>.Empty);
        }

        private static readonly BsonDocument Empty = new BsonDocument();
        [Benchmark]
        public async Task<GeoIp> NewClientFirstOrDefault()
        {
            var result = await _collection.Find(Empty).FirstOrDefaultAsync();
            return result;
        }

        [Benchmark]
        public async Task<GeoIp> OldClientFirstOrDefault()
        {
            var result = await _oldCollection.Find(FilterDefinition<GeoIp>.Empty).FirstOrDefaultAsync();
            return result;
        }
    }
}
