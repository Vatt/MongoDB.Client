using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.ConsoleApp
{
    public class ComplexBenchmarkBase<T> where T : IIdentified
    {
        private MongoCollection<T> _collection;
        private T[] _items;
        private readonly MongoDatabase _database;

        public int RequestsCount { get; }
        public int Parallelism { get; }

        public ComplexBenchmarkBase(MongoDatabase database, int parallelism, int requestsCount)
        {
            _database = database;
            Parallelism = parallelism;
            RequestsCount = requestsCount;
        }

        public void Setup()
        {
            _collection = _database.GetCollection<T>("Insert" + Guid.NewGuid().ToString());

            _items = new DatabaseSeeder().GenerateSeed<T>(RequestsCount).ToArray();
            var set = new HashSet<BsonObjectId>();
            foreach (var item in _items)
            {
                if (set.Add(item.Id) == false)
                {
                    throw new Exception("Duplicate id");
                }
            }
        }


        public async Task Clean()
        {
            await _collection.DropAsync();
        }

        public async Task Run()
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
                var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(1) { SingleWriter = true });
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
                        try
                        {
                            await Work(collection, item);
                        }
                        catch (Exception)
                        {
                            // skip
                        }

                    }
                }
                catch (ChannelClosedException)
                {
                    // channel complete
                }
            }
        }
    }
}
