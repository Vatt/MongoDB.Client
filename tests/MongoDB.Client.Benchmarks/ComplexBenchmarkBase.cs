﻿using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Experimental;
using MongoDB.Client.Tests.Models;
using MongoDB.Driver;
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
        private IMongoCollection<T> _oldCollection;
        private T[] _items;

        [Params(1024)]
        public int RequestsCount { get; set; }

        [Params(1, 4, 8, 16, 32, 64, 128, 256, 512)] public int Parallelism { get; set; }

        [Params(ClientType.Old, ClientType.New, ClientType.NewExperimental)]
        public ClientType ClientType { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = "Complex" + typeof(T).Name + Guid.NewGuid().ToString();
            _items = new DatabaseSeeder().GenerateSeed<T>(RequestsCount).ToArray();

            switch (ClientType)
            {
                case ClientType.Old:
                    InitOldClient(host, dbName, collectionName);
                    break;
                case ClientType.New:
                    await InitNewClient(host, dbName, collectionName);
                    break;
                case ClientType.NewExperimental:
                    await InitNewClientExperimental(host, dbName, collectionName);
                    break;
                default:
                    throw new NotSupportedException(ClientType.ToString());
            }

            //await InitNewClient(host, dbName, collectionName);
            //await InitNewClientExperimental(host, dbName, collectionName);
        }

        private async Task InitNewClient(string host, string dbName, string collectionName)
        {
            var client = new MongoClient(new DnsEndPoint(host, 27017));
            await client.InitAsync();
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<T>(collectionName);
        }
        private async Task InitNewClientExperimental(string host, string dbName, string collectionName)
        {
            var client = MongoExperimental.CreateWithExperimentalConnection(new DnsEndPoint(host, 27017), new NullLoggerFactory());
            await client.InitAsync();
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<T>(collectionName);
        }
        private void InitOldClient(string host, string dbName, string collectionName)
        {
            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<T>(collectionName);
        }

        [GlobalCleanup]
        public async Task Clean()
        {
            switch (ClientType)
            {
                case ClientType.Old:
                    await _oldCollection.Database.DropCollectionAsync(_oldCollection.CollectionNamespace.CollectionName);
                    break;
                case ClientType.New:
                    await _collection.DropAsync();
                    break;
                case ClientType.NewExperimental:
                    await _collection.DropAsync();
                    break;
                default:
                    throw new NotSupportedException(ClientType.ToString());
            }
        }

        [Benchmark]
        public async Task ComplexBenchmark()
        {
            switch (ClientType)
            {
                case ClientType.Old:
                    await StartOld(_oldCollection, _items);
                    break;
                case ClientType.New:
                    await StartNew(_collection, _items);
                    break;
                case ClientType.NewExperimental:
                    await StartNew(_collection, _items);
                    break;
                default:
                    throw new NotSupportedException(ClientType.ToString());
            }
        }

        private async Task StartNew(MongoCollection<T> collection, T[] items)
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
            }
        }

        private async Task StartOld(IMongoCollection<T> collection, T[] items)
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

            static async Task Work(IMongoCollection<T> collection, T item)
            {
                await collection.InsertOneAsync(item);
                await collection.Find(i => i.OldId == item.OldId).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(i => i.OldId == item.OldId);
            }

            static async Task Worker(IMongoCollection<T> collection, ChannelReader<T> reader)
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
            }
        }
    }
}