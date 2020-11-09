using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace MongoDB.Client.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .SetMinimumLevel(LogLevel.Error)
                    .AddConsole();
            });


            var client = new MongoClient(new DnsEndPoint(host, 27017), loggerFactory);

            var db = client.GetDatabase("TestDb");
            var collection1 = db.GetCollection<GeoIp>("TestCollection4");
            // var collection3 = db.GetCollection<BsonDocument>("TestCollection2");
            // var collection2 = db.GetCollection<GeoIp>("TestCollection3");
            var filter = new BsonDocument();
            //  var filter = new BsonDocument("_id", new BsonObjectId("5fa29b6db27162107ffbe7db"));

            var result0 = await collection1.Find(filter).ToListAsync();
            var result1 = await collection1.Find(filter).FirstOrDefaultAsync();


            var item = CreateItem();
            await collection1.InsertAsync(item);
            
            Console.WriteLine();
            // var result1 = await collection2.GetCursorAsync(filter, default);
            //
            //
            // var filter2 = new BsonDocument("_id", new BsonObjectId("5fa29b6db27162107ffbe7db"));
            // var filter3 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294a"));
            // var filter4 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57295c"));
            // var result2 = await collection1.GetCursorAsync(filter2, default);
            //
            //
            //
            // Console.WriteLine(result0.Cursor.Items.Count);
            // Console.WriteLine(result1.Cursor.Items.Count);
            // Console.WriteLine(result2.Cursor.Items.Count);
            // await Warmup(collection1, filter);


            var count = 1000;
            await Concurrent(collection1, count, filter);
            await Sequential(collection1, count, filter);
            await Concurrent(collection1, count, filter);
            await Sequential(collection1, count, filter);
            await Concurrent(collection1, count, filter);
            await Sequential(collection1, count, filter);
            // await Concurrent(collection1, count, filter);
            // await Sequential(collection1, count, filter);
            // await Concurrent(collection1, count, filter);
            // await Sequential(collection1, count, filter);
            // await Concurrent(collection1, count, filter);
            // await Sequential(collection1, count, filter);
        }

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