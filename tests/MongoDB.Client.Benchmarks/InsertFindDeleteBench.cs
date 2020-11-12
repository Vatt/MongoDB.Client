using System;
using System.Net;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;
using BsonObjectId = MongoDB.Client.Bson.Document.BsonObjectId;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertFindDeleteBench
    {
        private MongoCollection<GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        private GeoIp _item = new GeoIp
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

        [GlobalSetup]
        public void Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = GetType().Name;

            var client = new MongoClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<GeoIp>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<GeoIp>(collectionName);
        }

        [GlobalCleanup]
        public void Clean()
        {
            _oldCollection.DeleteMany(FilterDefinition<GeoIp>.Empty);
        }

        private static readonly BsonDocument Empty = new BsonDocument();

        [Benchmark]
        public async Task NewClient()
        {
            _item.Id = BsonObjectId.NewObjectId();
            await _collection.InsertAsync(_item);
            var result = await _collection.Find(Empty).FirstOrDefaultAsync();
            var deleteResult = await _collection.DeleteOneAsync(Empty);
        }

        [Benchmark]
        public async Task OldClient()
        {
            _item.OldId = ObjectId.GenerateNewId();
            await _oldCollection.InsertOneAsync(_item);
            var result = await _oldCollection.Find(FilterDefinition<GeoIp>.Empty).FirstOrDefaultAsync();
            var deleteResult = await _oldCollection.DeleteOneAsync(FilterDefinition<GeoIp>.Empty);
        }
    }
}