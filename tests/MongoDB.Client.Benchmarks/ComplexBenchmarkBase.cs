using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Client.Bson.Document;
using System;
using System.Linq;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class ComplexBenchmarkBase<T> where T : IIdentified
    {
        private MongoCollection<T> _collection;
        private T[] _items;

        [Params(1024)]
        public int RequestsCount { get; set; }

        [Params(1, 4, 8, 16, 32, 64, 128, 256, 512)] public int Parallelism { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var client = new MongoClient(new DnsEndPoint(host, 27017));
            await client.InitAsync();
            var db = client.GetDatabase(dbName);

 
            _collection = db.GetCollection<T>("Insert" + Guid.NewGuid().ToString());

            _items = new DatabaseSeeder().GenerateSeed<T>(RequestsCount).ToArray();
        }


        [GlobalCleanup]
        public async Task Clean()
        {
            await _collection.DropAsync();
        }

        protected async Task Run()
        {
            await Start(_collection, _items);
        }

        private async Task Start(MongoCollection<T> collection, T[] items)
        {
            if (Parallelism == 1)
            {
                foreach (var item in items)
                {
                    await Work(collection, item);
                }
            }
            else
            {
                var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(collection, channel.Reader);
                }

                foreach (var item in items)
                {
                    await channel.Writer.WriteAsync(item);
                }

                channel.Writer.Complete();
                await Task.WhenAll(tasks);
            }

            static async Task Work(MongoCollection<T> collection, T item)
            {
                var filter = new BsonDocument("_id", item.Id);
                await collection.InsertAsync(item);
                await collection.Find(filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(filter);
            }

            static async Task Worker(MongoCollection<T> collection, ChannelReader<T> reader)
            {
                try
                {
                    while (true)
                    {
                        var item = await reader.ReadAsync();
                        await Work(collection, item);
                    }
                }
                catch (ChannelClosedException)
                {
                    // channel complete
                }

                //while (await reader.WaitToReadAsync())
                //{
                //    while (reader.TryRead(out var item))
                //    {
                //        await Work(collection, item);
                //    }
                //}
            }
        }
    }
}