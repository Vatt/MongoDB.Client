using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new MongoClient(/*new DnsEndPoint("centos0.mshome.net", 27017)*/);
            var connectionInfo = await client.ConnectAsync(default);
           
            var filter = new BsonDocument();
            
            var result0 = await client.GetCursorAsync<GeoIp>("TestDb", "TestCollection3", filter, default);
            var result1 = await client.GetCursorAsync<GeoIp>("TestDb", "TestCollection2", filter, default);



            var filter2 = new BsonDocument("_id", new BsonObjectId("5f987814bf344ec7cc57294b"));
            var result2 = await client.GetCursorAsync<GeoIp>("TestDb", "TestCollection2", filter2, default);


            await client.DisposeAsync();

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
