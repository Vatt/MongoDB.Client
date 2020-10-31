using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Bson.Document;
using MongoDB.Driver;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class RequestsBench
    {
        private MongoCollection<MongoDB.Client.GeoIp> _collection;
        private IMongoCollection<GeoIp> _oldCollection;

        [GlobalSetup]
        public void Setup()
        {
            var client = new MongoClient();
            var db = client.GetDatabase("TestDb");
            _collection = db.GetCollection<MongoDB.Client.GeoIp>("TestCollection2");

            var oldClient = new MongoDB.Driver.MongoClient("mongodb://localhost:27017");
            var oldDb = oldClient.GetDatabase("TestDb");
            _oldCollection = oldDb.GetCollection<GeoIp>("TestCollection2");
        }

        [Benchmark]
        public async Task<int> NewClient()
        {
            var filter = new BsonDocument();

            var result = await _collection.GetCursorAsync(filter, default);
            return result.Cursor.Items.Count;
        }
        
        [Benchmark]
        public async Task<int> OldClient()
        {
            var result232 = await _oldCollection.Find(FilterDefinition<GeoIp>.Empty).ToListAsync();
            return result232.Count;
        }
    }
}