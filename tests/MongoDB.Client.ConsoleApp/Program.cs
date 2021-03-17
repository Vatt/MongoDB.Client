using Microsoft.Extensions.Logging;
using MongoDB.Client.Tests.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MongoDB.Client.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {

            //await LoadTest<GeoIp>(1024*1024, new[] { 512 });
            // await ReplicaSetConenctionTest<GeoIp>(1024*4, new[] { 4 }, true);
            await TestTransaction();
            Console.WriteLine("Done");
        }


        static async Task TestTransaction()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            //var client = await MongoClient.CreateClient("mongodb://centos.mshome.net:27018,centos.mshome.net:27019,centos.mshome.net:27020/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=secondaryPreferred", loggerFactory);
            var client = await MongoClient.CreateClient("mongodb://centos1.mshome.net,centos2.mshome.net,centos3.mshome.net/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=secondaryPreferred", loggerFactory);
            var db = client.GetDatabase("TestDb");

            await db.DropCollectionAsync("TransactionCollection");
            await db.CreateCollectionAsync("TransactionCollection");
            var collection = db.GetCollection<GeoIp>("TransactionCollection");

            await WithoutTx(collection);
            await WithCommitTx(collection);
            await WithAbortTx(collection);

            async Task WithoutTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(1).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await collection.InsertAsync(item);
                var result = await collection.Find(filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(filter);
            }

            async Task WithCommitTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(1).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await using var transaction = client.StartTransaction();

                await collection.InsertAsync(transaction, item);
                var result = await collection.Find(transaction, filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(transaction, filter);

                await transaction.CommitAsync();
            }


            async Task WithAbortTx(MongoCollection<GeoIp> collection)
            {
                var item = new GeoIpSeeder().GenerateSeed(1).First();
                var filter = new Bson.Document.BsonDocument("_id", item.Id);

                await using var transaction = client.StartTransaction();

                await collection.InsertAsync(transaction, item);
                var result = await collection.Find(transaction, filter).FirstOrDefaultAsync();
                await collection.DeleteOneAsync(transaction, filter);

                await transaction.AbortAsync();
            }
        }

        static async Task ReplicaSetConenctionTest<T>(int requestCount, IEnumerable<int> parallelism, bool useTransaction) where T : IIdentified
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            var client = await MongoClient.CreateClient("mongodb://centos.mshome.net:27018,centos.mshome.net:27019,centos.mshome.net:27020/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=Primary", loggerFactory);
            var db = client.GetDatabase("TestDb");
            var stopwatch = new Stopwatch();
            Console.WriteLine(typeof(T).Name);
            foreach (var item in parallelism)
            {
                Console.WriteLine("Start: " + item);
                var bench = new ComplexBenchmarkBase<T>(db, item, requestCount);
                bench.Setup();

                stopwatch.Restart();
                try
                {
                    await bench.Run(useTransaction);
                    stopwatch.Stop();
                }
                finally
                {
                    await bench.Clean();
                }

                Console.WriteLine($"End: {item}. Elapsed: {stopwatch.Elapsed}");
            }
        }
        static async Task LoadTest<T>(int requestCount, IEnumerable<int> parallelism) where T : IIdentified
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });

            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017), loggerFactory);
            // var client = MongoExperimental.CreateWithExperimentalConnection(new DnsEndPoint(host, 27017), loggerFactory);

            var db = client.GetDatabase("TestDb");
            var stopwatch = new Stopwatch();
            Console.WriteLine(typeof(T).Name);
            foreach (var item in parallelism)
            {
                Console.WriteLine("Start: " + item);
                var bench = new ComplexBenchmarkBase<T>(db, item, requestCount);
                bench.Setup();

                stopwatch.Restart();
                try
                {
                    await bench.Run(false);
                    stopwatch.Stop();
                }
                finally
                {
                    await bench.Clean();
                }

                Console.WriteLine($"End: {item}. Elapsed: {stopwatch.Elapsed}");
            }

            Console.WriteLine("Done");
        }
    }
}
