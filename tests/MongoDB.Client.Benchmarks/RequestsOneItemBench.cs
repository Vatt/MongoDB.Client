using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;
using BsonObjectId = MongoDB.Client.Bson.Document.BsonObjectId;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsOneItemBench
    {
        private MongoCollection<GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        [GlobalSetup]
        public void Setup()
        {
            var dbName = "BenchmarkDb";
            var collectionName = "OneItemBench";
            
            var client = new MongoClient();
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<GeoIp>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient("mongodb://localhost:27017");
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