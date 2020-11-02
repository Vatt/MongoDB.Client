using MongoDB.Client.Bson.Document;
using System;
using System.Collections.Generic;
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

            var result0 = await collection1.GetCursorAsync<GeoIp>(filter, default);
            var result1 = await collection2.GetCursorAsync<GeoIp>(filter, default);


            var filter2 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294b"));
            var result2 = await collection1.GetCursorAsync<GeoIp>(filter2, default);

            Console.WriteLine(result0.Cursor.Items.Count);
            Console.WriteLine(result1.Cursor.Items.Count);
            Console.WriteLine(result2.Cursor.Items.Count);
            List<int> lst = new List<int>();
            for(var i = 0; i < lst.Count; i++)  
            {

            }
        }
    }
}