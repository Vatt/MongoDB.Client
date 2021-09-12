using System.Net;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Tests.Models;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class InsertBenchmark
    {
        private MongoCollection<GeoIp> _collection;
        [Params(4096)]
        public int RequestsCount { get; set; }

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
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = Guid.NewGuid().ToString();

            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<GeoIp>(collectionName);
        }

        [Benchmark]
        public async Task NewClientInsertFindRemove()
        {
            for (int i = 0; i < RequestsCount; i++)
            {
                _item.Id = MongoDB.Client.Bson.Document.BsonObjectId.NewObjectId();
                await _collection.InsertAsync(_item);
            }
        }
    }
}
