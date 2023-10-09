using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Models;

namespace MongoDB.Client.ConsoleApp
{
    public class ComplexBenchmarkBase<T> where T : IIdentified, IBsonSerializer<T>
    {
        private MongoCollection<T> _collection;
        private IEnumerable<T> _items;
        private readonly MongoDatabase _database;
        private readonly ILogger _logger;
        private readonly SeederOptions _options;

        public int Parallelism { get; }

        public ComplexBenchmarkBase(MongoDatabase database, int parallelism, ILogger logger, SeederOptions options)
        {
            _database = database;
            Parallelism = parallelism;
            _options = options;
            _logger = logger;
        }

        public async Task Setup()
        {
            var collectionName = "Insert" + Guid.NewGuid().ToString();
            await _database.CreateCollectionAsync(collectionName);
            _collection = _database.GetCollection<T>(collectionName);
            var sw = Stopwatch.StartNew();
            _items = new DatabaseSeeder().GenerateSeed<T>(_options);
            _logger.LogInformation("Generating models completed in {Elapsed}", sw.Elapsed);
        }


        public async Task Clean()
        {
            await _collection.DropAsync();
        }

        public async Task Run(bool useTransaction)
        {
            await Start(_collection, _items, useTransaction);
        }

        private async Task Start(MongoCollection<T> collection, IEnumerable<T> items, bool useTransaction)
        {
            if (Parallelism == 1)
            {
                foreach (var item in items)
                {
                    await Work(collection, item, useTransaction);
                }
            }
            else
            {
                var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(Parallelism * 2) { SingleWriter = true });
                //var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Task.Run(() => Worker(collection, channel.Reader, useTransaction));
                }

                foreach (var item in items)
                {
                    if (channel.Writer.TryWrite(item) == false)
                    {
                        await channel.Writer.WriteAsync(item);
                    }
                }

                channel.Writer.Complete();
                await Task.WhenAll(tasks);
            }

            static async Task Work(MongoCollection<T> collection, T item, bool useTransaction)
            {
                var filter = new BsonDocument("_id", item.Id);
                if (useTransaction)
                {
                    await using var transaction = collection.Database.Client.StartTransaction();
                    await collection.InsertAsync(transaction, item);
                    var result = await collection.Find(transaction, filter).FirstOrDefaultAsync();
                    await collection.DeleteOneAsync(transaction, filter);
                    await transaction.CommitAsync();
                }
                else
                {
                    await collection.InsertAsync(item);
                    var result = await collection.Find(filter).FirstOrDefaultAsync();
                    await collection.DeleteOneAsync(filter);
                }
            }

            static async Task Worker(MongoCollection<T> collection, ChannelReader<T> reader, bool useTransaction)
            {
                await foreach (var item in reader.ReadAllAsync())
                {
                    await Work(collection, item, useTransaction);
                }
            }
        }
    }
}
