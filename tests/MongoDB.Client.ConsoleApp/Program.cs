using Microsoft.Extensions.Logging;
using MongoDB.Bson;
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
            //await ReplicaSetConenctionTest<GeoIp>(1024, new[] { 1 });
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
            var client = new MongoClient("mongodb://centos.mshome.net:27018,centos.mshome.net:27019,centos.mshome.net:27020/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=Primary", loggerFactory);
            await client.InitAsync();
            var db = client.GetDatabase("TestDb");
            var collection = db.GetCollection<GeoIp>("TransactionCollection");
            var item = new GeoIpSeeder().GenerateSeed(1).First();

            await using var transaction = client.StartTransaction();

            await collection.InsertAsync(transaction, item);
            var filter = new Bson.Document.BsonDocument("_id", item.Id);
            var result =  await collection.Find(transaction, filter).FirstOrDefaultAsync();
            await collection.DeleteOneAsync(transaction, filter);

            await transaction.CommitAsync();
        }

        static async Task ReplicaSetConenctionTest<T>(int requestCount, IEnumerable<int> parallelism) where T : IIdentified
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Information)
                    .AddConsole();
            });
            var client = new MongoClient("mongodb://centos.mshome.net:27018,centos.mshome.net:27019,centos.mshome.net:27020/?replicaSet=rs0&maxPoolSize=9&appName=MongoDB.Client.ConsoleApp&readPreference=Secondary", loggerFactory);
           // var client = new MongoClient("mongodb://centos.mshome.net:27017/", loggerFactory);
            await client.InitAsync();
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
                    await bench.Run();
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

            var client = new MongoClient(new DnsEndPoint(host, 27017), loggerFactory);
            // var client = MongoExperimental.CreateWithExperimentalConnection(new DnsEndPoint(host, 27017), loggerFactory);
            await client.InitAsync();
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
                    await bench.Run();
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