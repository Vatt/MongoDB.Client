using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class ComplexBenchmark
    {
        private MongoCollection<RootDocument> _findCollection;
        private MongoCollection<GeoIp> _insertDeleteCollection;
        private List<GeoIp> _insertDocs;
        [Params(1024)]
        public int RequestsCount { get; set; }

        [Params(2048 * 2)]
        public int ItemInDb { get; set; }

        [Params(1, 4, 8, 16, 32, 64, 128)] public int Parallelism { get; set; }

        [GlobalSetup]
        public async Task Setup()
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
            var dbName = "BenchmarkDb";
            var client = new MongoClient(new DnsEndPoint(host, 27017));
            var db = client.GetDatabase(dbName);
            
            _findCollection = db.GetCollection<RootDocument>(Guid.NewGuid().ToString());
            _insertDeleteCollection = db.GetCollection<GeoIp>("InsertDelete" + Guid.NewGuid().ToString());
            var seeder = new DatabaseSeeder();
            var geoipseeder = new GeoIpSeeder();
            _insertDocs = geoipseeder.GenerateSeed(ItemInDb).ToList();
            foreach (var item in seeder.GenerateSeed(ItemInDb))
            {
                await _findCollection.InsertAsync(item);
            }
        }
        [Benchmark]
        public async Task ComplexBenchmarkNewClient()
        {
            Channel<BsonObjectId> deleteChannel;
            if (Parallelism == 1)
            {
                deleteChannel = Channel.CreateUnbounded<BsonObjectId>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = true });
            }
            else
            {
                deleteChannel = Channel.CreateUnbounded<BsonObjectId>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = false });
            }
            
            await Task.WhenAll(StartFind(), StartDelete(deleteChannel), StartInsert(deleteChannel));
        }
        public async Task StartInsert(Channel<BsonObjectId>  deleteChannel)
        {
            if (Parallelism == 1)
            {
                var writer = deleteChannel.Writer;
                for (int i = 0; i < ItemInDb; i++)
                {
                    var item = _insertDocs[i];
                    item.Id = BsonObjectId.NewObjectId();
                    await _insertDeleteCollection.InsertAsync(item);
                    await writer.WriteAsync(item.Id);

                }
                writer.Complete();
            }
            else
            {
                var channel = Channel.CreateUnbounded<GeoIp>(new UnboundedChannelOptions { SingleWriter = true });
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(_insertDeleteCollection, deleteChannel.Writer, channel.Reader);
                }

                for (int i = 0; i < ItemInDb; i++)
                {
                    var item = _insertDocs[i];
                    item.Id = BsonObjectId.NewObjectId();
                    await channel.Writer.WriteAsync(item);
                }
                channel.Writer.Complete();
                await Task.WhenAll(tasks);
                deleteChannel.Writer.Complete();
            }

            static async Task Worker(MongoCollection<GeoIp> collection, ChannelWriter<BsonObjectId> deleteWriter, ChannelReader<GeoIp> reader)
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var item))
                    {

                        await collection.InsertAsync(item);
                        await deleteWriter.WriteAsync(item.Id);
                    }
                }
            }
        }
        public async Task StartDelete(Channel<BsonObjectId> deleteChannel)
        {
            if (Parallelism == 1)
            {
                var reader = deleteChannel.Reader;
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out var item))
                    {
                        await _insertDeleteCollection.DeleteOneAsync(BsonDocument.Empty/*new BsonDocument("Id", id)*/);
                    }
                        
                }
            }
            else
            {
                var tasks = new Task[Parallelism];
                for (int i = 0; i < Parallelism; i++)
                {
                    tasks[i] = Worker(_insertDeleteCollection, deleteChannel.Reader);
                }
                await Task.WhenAll(tasks);
            }
            static async Task Worker(MongoCollection<GeoIp> collection, ChannelReader<BsonObjectId> reader)
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