﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class SuperComplexBenchmark
    {
        private MongoCollection<RootDocument> _findCollection;
        private MongoCollection<GeoIp> _deleteCollection;
        private MongoCollection<GeoIp> _insertCollection;
        private List<GeoIp> _insertDocs;
        [Params(1024)]
        public uint RequestsCount { get; set; }

        [Params(2048 * 2)]
        public uint ItemInDb { get; set; }

        [Params(1, 4, 8, 16, 32, 64, 128)] public int Parallelism { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);

            _findCollection = db.GetCollection<RootDocument>(Guid.NewGuid().ToString());
            _insertCollection = db.GetCollection<GeoIp>("Insert" + Guid.NewGuid().ToString());
            _deleteCollection = db.GetCollection<GeoIp>("Delete" + Guid.NewGuid().ToString());
            var seeder = new RootDocumentSeeder();
            var geoipseeder = new GeoIpSeeder();
            _insertDocs = geoipseeder.GenerateSeed(SeederOptions.Create(ItemInDb)).ToList();
            foreach (var item in _insertDocs)
            {
                await _deleteCollection.InsertAsync(item);
            }
            foreach (var item in seeder.GenerateSeed(SeederOptions.Create(ItemInDb)))
            {
                await _findCollection.InsertAsync(item);
            }
        }


        [GlobalCleanup]
        public async Task Clean()
        {
            await _findCollection.DropAsync();
            await _deleteCollection.DropAsync();
            await _insertCollection.DropAsync();
        }


        [Benchmark]
        public async Task ComplexBenchmarkNewClient()
        {
            await Task.WhenAll(StartFind(), StartDelete(), StartInsert());
        }
        public async Task StartInsert()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < ItemInDb; i++)
                {
                    var item = _insertDocs[i];
                    item.Id = BsonObjectId.NewObjectId();
                    await _insertCollection.InsertAsync(item);
                    await _insertCollection.DeleteOneAsync(BsonDocument.Empty);
                }
            }
            else
            {
                var channel = Channel.CreateUnbounded<GeoIp>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(_insertCollection, channel.Reader);
                }

                for (int i = 0; i < ItemInDb; i++)
                {
                    var item = _insertDocs[i];
                    item.Id = BsonObjectId.NewObjectId();
                    await channel.Writer.WriteAsync(item);
                }
                channel.Writer.Complete();
                await Task.WhenAll(tasks);
            }

            static async Task Worker(MongoCollection<GeoIp> collection, ChannelReader<GeoIp> reader)
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var item))
                    {
                        await collection.InsertAsync(item);
                        await collection.DeleteOneAsync(BsonDocument.Empty);
                    }
                }
            }
        }
        public async Task StartDelete()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < ItemInDb; i++)
                {
                    var item = _insertDocs[i];
                    await _deleteCollection.DeleteOneAsync(BsonDocument.Empty);
                }
            }
            else
            {
                var channel = Channel.CreateUnbounded<GeoIp>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(_deleteCollection, channel.Reader);
                }
                for (int i = 0; i < ItemInDb; i++)
                {
                    var item = _insertDocs[i];
                    await channel.Writer.WriteAsync(item);
                }
                channel.Writer.Complete();
                await Task.WhenAll(tasks);
            }
            static async Task Worker(MongoCollection<GeoIp> collection, ChannelReader<GeoIp> reader)
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var item))
                    {
                        await collection.DeleteOneAsync(BsonDocument.Empty/*new BsonDocument("Id", id)*/);
                    }
                }
            }
        }
        public async Task StartFind()
        {
            if (Parallelism == 1)
            {
                for (int i = 0; i < RequestsCount; i++)
                {
                    await _findCollection.Find(BsonDocument.Empty).FirstOrDefaultAsync();
                }
            }
            else
            {
                var channel = Channel.CreateUnbounded<BsonDocument>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(_findCollection, channel.Reader);
                }

                for (int i = 0; i < RequestsCount; i++)
                {
                    channel.Writer.TryWrite(BsonDocument.Empty);
                }
                channel.Writer.Complete();
                await Task.WhenAll(tasks);
            }

            static async Task Worker(MongoCollection<RootDocument> collection, ChannelReader<BsonDocument> reader)
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var filter))
                    {
                        await collection.Find(filter).FirstOrDefaultAsync();
                    }
                }
            }
        }
    }

}
