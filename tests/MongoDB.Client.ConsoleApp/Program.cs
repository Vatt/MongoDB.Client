using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.ConsoleApp.Models;
using System;
using System.Buffers;
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
            // var loggerFactory = LoggerFactory.Create(builder =>
            // {
            //     builder
            //         .SetMinimumLevel(LogLevel.Error)
            //         .AddConsole();
            // });
            // //var client = new MongoClient("mongodb://centos1.mshome.net, centos2.mshome.net, centos3.mshome.net/?replicaSet=rs0&maxPoolSize=32&appName=MongoDB.Client.ConsoleApp", loggerFactory);
            // var client = new MongoClient("mongodb://centos0.mshome.net/?maxPoolSize=32&appName=MongoDB.Client.ConsoleApp", loggerFactory);
            // await client.InitAsync();
            // var db = client.GetDatabase("TestDb");
            // var collection1 = db.GetCollection<RootDocument>("HeavyItems");
            //
            //
            // var filter = new BsonDocument();
            //
            // var seeder = new DatabaseSeeder();
            // var item = seeder.GenerateSeed().First();
            //
            // await collection1.InsertAsync(item);
            //
            // Console.WriteLine("Done");
            // //var item = seeder.GenerateSeed().First();
            // //var settings = 
            // //     MongoClientSettings.FromConnectionString(
            // //         @"mongodb://login:password@10.19.10.19:27117,10.19.10.19:27118,10.19.10.19:27119/?
            // //         readPreference=primary&replicaSet=rs0&connectTimeoutMS=300000&socketTimeoutMS=30000&
            // //         ssl=false&maxPoolSize=100");
            // //var settings = MongoClientSettings.FromConnectionString("mongodb://%2Ftmp%2Fmongodb-27017.sock");
            // //var settings = MongoClientSettings.FromConnectionString("mongodb://login:password@10.19.10.19:27117,10.19.10.19:27118,10.19.10.19:27119");
            // //var settings = MongoClientSettings.FromConnectionString("mongodb://10.19.10.19:27117,10.19.10.19:27118,10.19.10.19:27119");
            // //var settings = MongoClientSettings.FromConnectionString("mongodb://10.19.10.19:27117,10.19.10.19,10.19.10.19:27119");
            // //var settings = MongoClientSettings.FromConnectionString("mongodb://10.19.10.19:27117");

            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Error)
                    .AddConsole();
            });
            
            var client = new MongoClient(new DnsEndPoint(host, 27017), loggerFactory);
            await client.InitAsync();
            var db = client.GetDatabase("TestDb");
            
            var collection1 = db.GetCollection<RootDocument>("TestColl");


            var filter = new BsonDocument();

            var seeder = new DatabaseSeeder();
            var item = seeder.GenerateSeed().First();

            await collection1.InsertAsync(item);
            await db.DropCollectionAsync("TestColl");

            Console.WriteLine("Done");
        }

        public static void TestMockPipe()
        {
            var seeder = new DatabaseSeeder();
            var item = seeder.GenerateSeed().First();
            SerializersMap.TryGetSerializer<RootDocument>(out var serializer);
            var pipe = new ArrayBufferWriter<byte>(1024 * 1024);


            var writer = new BsonWriter(pipe);
            serializer.WriteBson(ref writer, item);


            var reader = new BsonReader(pipe.WrittenMemory);
            serializer.TryParseBson(ref reader, out var parsedItem);

            var eq = item.Equals(parsedItem);
        }
        //TODO: FIXIT!
        //private static async Task InsertItems(MongoCollection<GeoIp> collection, int count)
        //{
        //    for (int i = 0; i < count; i++)
        //    {
        //        var item = CreateItem();
        //        await collection.InsertAsync(item);
        //    }
        //}


        private static GeoIp CreateItem()
        {
            var geoIp = new GeoIp
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
            return geoIp;
        }

        private static async Task Concurrent<T>(MongoCollection<T> collection, int count, BsonDocument filter)
        {
            var list = new List<Task<List<T>>>(count);
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                list.Add(collection.Find(filter).ToListAsync().AsTask());
            }

            var results = await Task.WhenAll(list);
            foreach (var result in results)
            {
                if (result.Count != 1100)
                {
                    Console.WriteLine("Result length: " + result.Count);
                }
            }

            sw.Stop();
            Console.WriteLine("Concurrent: " + sw.Elapsed);
        }

        private static async Task Sequential<T>(MongoCollection<T> collection, int count, BsonDocument filter)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                // using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                // {
                //     try
                //     {
                var result = await collection.Find(filter).ToListAsync();
                if (result.Count != 1100)
                {
                    Console.WriteLine("Result length: " + result.Count);
                }

                //     }
                //     catch (Exception e)
                //     {
                //         await Console.Out.WriteLineAsync(e.Message);
                //     }
                // }
                //
                // await Console.Out.WriteLineAsync(i.ToString());
            }

            sw.Stop();
            Console.WriteLine("Sequential: " + sw.Elapsed);
        }


        private static async Task Warmup<T>(MongoCollection<T> collection, BsonDocument filter)
        {
            for (int i = 0; i < 1000; i++)
            {
                // using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                // {
                //     try
                //     {
                var result = await collection.Find(filter).ToListAsync();
                //     }
                //     catch (Exception e)
                //     {
                //         await Console.Out.WriteLineAsync(e.Message);
                //     }
                // }
                //
                // await Console.Out.WriteLineAsync(i.ToString());
            }
        }
    }
}