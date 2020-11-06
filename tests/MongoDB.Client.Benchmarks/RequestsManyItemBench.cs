using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;
using BsonObjectId = MongoDB.Client.Bson.Document.BsonObjectId;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsManyItemBench
    {
        private MongoCollection<GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        [GlobalSetup]
        public void Setup()
        {
            var dbName = "TestDb";
            var collectionName = "TestCollection4";
            
            var client = new MongoClient();
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<GeoIp>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient("mongodb://localhost:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<GeoIp>(collectionName);
        }

        private static readonly BsonDocument EmptyFilter = new BsonDocument();
        
        [Benchmark]
        public async Task<int> NewClient()
        {
            var result = await _collection.Find(EmptyFilter).ToListAsync();
            return result.Count;
        }
        
        [Benchmark]
        public async Task<int> OldClient()
        {
            var result232 = await _oldCollection.Find(FilterDefinition<GeoIp>.Empty).ToListAsync();
            return result232.Count;
        }
    }
}