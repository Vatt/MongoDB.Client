using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Client.Bson.Document;
using System;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class ComplexBenchmarkWithLargeFind
    {
        private MongoCollection<GeoIp> _collection;
        private MongoCollection<GeoIp> _findCollection;
        private GeoIp[] _items;

        [Params(1024)]
        public int RequestsCount { get; set; }

        [Params(1, 4, 8, 16, 32, 64, 128)] public int Parallelism { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var client = new MongoClient(new DnsEndPoint(host, 27017));
            await client.InitAsync();
            var db = client.GetDatabase(dbName);


            _collection = db.GetCollection<GeoIp>(Guid.NewGuid().ToString());
            _findCollection = db.GetCollection<GeoIp>("Find" + Guid.NewGuid().ToString());

            _items = new GeoIp[RequestsCount];
            for (int i = 0; i < RequestsCount; i++)
            {
                _items[i] = new GeoIp
                {
                    Id = BsonObjectId.NewObjectId(),
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
            }

            await _findCollection.InsertAsync(_items);
        }


        [GlobalCleanup]
        public async Task Clean()
        {
            await _collection.DropAsync();
        }


        [Benchmark]
        public async Task ComplexBenchmarkNewClient()
        {
            await Start();
        }


        public async Task Start()
        {
            if (Parallelism == 1)
            {
                foreach (var item in _items)
                {
                    await Work(_collection, _findCollection, item);
                }
            }
            else
            {
                var channel = Channel.CreateUnbounded<GeoIp>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(_collection, _findCollection, channel.Reader);
                }

                foreach (var item in _items)
                {
                    await channel.Writer.WriteAsync(item);
                }

                channel.Writer.Complete();
                await Task.WhenAll(tasks);
            }

            static async Task Work(MongoCollection<GeoIp> collection, MongoCollection<GeoIp> findCollection, GeoIp item)
            {
                var filter = new BsonDocument("_id", item.Id);
                await collection.InsertAsync(item);
                await findCollection.Find(filter).ToListAsync();
                await collection.DeleteOneAsync(filter);
            }

            static async Task Worker(MongoCollection<GeoIp> collection, MongoCollection<GeoIp> findCollection, ChannelReader<GeoIp> reader)
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var item))
                    {
                        await Work(collection, findCollection, item);
                    }
                }
            }
        }
    }
}