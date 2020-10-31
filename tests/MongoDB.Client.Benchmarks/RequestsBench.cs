using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;
using BsonObjectId = MongoDB.Client.Bson.Document.BsonObjectId;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsBench
    {
        private MongoCollection<GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        [GlobalSetup]
        public void Setup()
        {
            var client = new MongoClient();
            var db = client.GetDatabase("TestDb");
            _collection = db.GetCollection<GeoIp>("TestCollection2");

            var oldClient = new MongoDB.Driver.MongoClient("mongodb://localhost:27017");
            var oldDb = oldClient.GetDatabase("TestDb");
            _oldCollection = oldDb.GetCollection<GeoIp>("TestCollection2");
        }

        [Benchmark]
        public async Task<int> NewClient()
        {
            var result = await _collection.GetCursorAsync(new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294b")), default).ToListAsync();
            return result.Count;
        }
        
        [Benchmark]
        public async Task<int> OldClient()
        {
            var result232 = await _oldCollection.Find(g => g.OldId == new ObjectId("5f987814bf344ec7cc57294b")).ToListAsync();
            return result232.Count;
        }
    }
}