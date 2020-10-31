using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


namespace MongoDB.Client.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new MongoClient( /*new DnsEndPoint("centos0.mshome.net", 27017)*/);

            var db = client.GetDatabase("TestDb");
            var collection1 = db.GetCollection<GeoIp>("TestCollection2");
            var collection2 = db.GetCollection<GeoIp>("TestCollection3");

            var filter = new BsonDocument();

            var result0 = await collection1.GetCursorAsync(filter, default);
            var result1 = await collection2.GetCursorAsync(filter, default);


            var filter2 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294b"));
            var filter3 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294a"));
            var filter4 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57295c"));
            var result2 = await collection1.GetCursorAsync(filter2, default);


    
            Console.WriteLine(result0.Cursor.Items.Count);
            Console.WriteLine(result1.Cursor.Items.Count);
            Console.WriteLine(result2.Cursor.Items.Count);


            var count = 8;
            await Concurrent(collection1, count, filter);
            //await Sequential(collection1, count, filter);
            //await Concurrent(collection1, count, filter);
            //await Sequential(collection1, count, filter);
        }

        private static async Task Concurrent<T>(MongoCollection<T> collection, int count, BsonDocument filter)
        {
            var list = new List<Task<T>>(count);
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                list.Add(collection.GetCursorAsync(filter, default).FirstOrDefaultAsync().AsTask());
            }
            var results = await Task.WhenAll(list);
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
        
        private static async Task Sequential<T>(MongoCollection<T> collection, int count, BsonDocument filter)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < count; i++)
            {
                await collection.GetCursorAsync(filter, default).FirstOrDefaultAsync();
            }
            sw.Stop();
            Console.WriteLine(sw.Elapsed);
        }
    }
}