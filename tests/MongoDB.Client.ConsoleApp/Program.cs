using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Test;

namespace MongoDB.Client.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new MongoClient(/*new DnsEndPoint("centos0.mshome.net", 27017)*/);

            var db = client.GetDatabase("TestDb");
            var collection1 = db.GetCollection<GeoIp>("TestCollection2");
            var collection2 = db.GetCollection<GeoIp>("TestCollection3");

            var filter = new BsonDocument();
            
            var result0 = await collection1.GetCursorAsync<GeoIp>( filter, default);
            var result1 = await collection2.GetCursorAsync<GeoIp>( filter, default);



            var filter2 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294b"));
            var result2 = await collection1.GetCursorAsync<GeoIp>(filter2, default);

            Console.WriteLine();
        }

        static void Test()
        {
            ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../ReaderTestCollection.bson");
            //ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../Meteoritelandings.bson");
            var reader = new BsonReader(file);
            //IBsonSerializable serializator = new MongoDB.Client.Test.Generated.NasaMeteoriteLandingGeneratedSerializator();
            //IBsonSerializable serializator = new MongoDB.Client.Bson.Serialization.Generated.DocumentObjectGeneratedSerializator();

            //serializator.TryParse(ref reader, out var doc);
            //reader.TryParseDocument(null, out var document);
            //reader.TryParseDocument(null, out var document1);
            //reader.TryParseDocument(null, out var document2);
            //reader.TryParseDocument(null, out var document3);
            //reader.TryParseDocument(null, out var document4);
            //reader.TryParseDocument(null, out var document5);
        }

    }
}
