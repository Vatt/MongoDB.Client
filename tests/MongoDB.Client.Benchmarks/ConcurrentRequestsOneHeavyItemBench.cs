﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Driver;
using BsonDocument = MongoDB.Client.Bson.Document.BsonDocument;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class ConcurrentRequestsOneHeavyItemBench
    {
        private MongoCollection<RootDocument> _collection;
        private IMongoCollection<RootDocument> _oldCollection;

        [Params(1, 4, 8, 16, 32, 64, 128)]
        public int Parallelism { get; set; }

        [Params(256)]
        public int RequestsCount { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var collectionName = GetType().Name;

            var client = new MongoClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            _collection = db.GetCollection<RootDocument>(collectionName);

            var oldClient = new MongoDB.Driver.MongoClient($"mongodb://{host}:27017");
            var oldDb = oldClient.GetDatabase(dbName);
            _oldCollection = oldDb.GetCollection<RootDocument>(collectionName);

            oldDb.DropCollection(collectionName);
            var item = new DatabaseSeeder().GenerateSeed(1).First();
            _oldCollection.InsertOne(item);
        }

        [GlobalCleanup]
        public void Clean()
        {
            _oldCollection.Database.DropCollection(GetType().Name);
        }

        private static readonly BsonDocument EmptyFilter = new BsonDocument();

        [Benchmark]
        public async Task NewClientFindFirstOrDefault()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < RequestsCount; i++)
                {
                    await _collection.Find(EmptyFilter).FirstOrDefaultAsync();
                }
            }
            else
            {
                var iterations = RequestsCount / Parallelism;
                var tasks = new Task<RootDocument>[Parallelism];
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < Parallelism; j++)
                    {
                        tasks[j] = _collection.Find(EmptyFilter).FirstOrDefaultAsync().AsTask();
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }

        [Benchmark]
        public async Task OldClientFindFirstOrDefault()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < RequestsCount; i++)
                {
                    await _oldCollection.Find(FilterDefinition<RootDocument>.Empty).FirstOrDefaultAsync();
                }
            }
            else
            {
                var iterations = RequestsCount / Parallelism;
                var tasks = new Task<RootDocument>[Parallelism];
                for (int i = 0; i < iterations; i++)
                {
                    for (int j = 0; j < Parallelism; j++)
                    {
                        tasks[j] = _oldCollection.Find(FilterDefinition<RootDocument>.Empty).FirstOrDefaultAsync();
                    }

                    await Task.WhenAll(tasks);
                }
            }
        }
    }
}