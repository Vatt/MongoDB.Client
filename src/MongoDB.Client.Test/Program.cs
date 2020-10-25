using MongoDB.Client.Bson.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{
    class Program
    {
        private static ReadOnlyMemory<byte> Hell2 = new byte[] { 55, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 0, 0, 0, 34, 0, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 2, 36, 100, 98, 0, 6, 0, 0, 0, 97, 100, 109, 105, 110, 0, 0 };
        private static ReadOnlyMemory<byte> Req1 = new byte[] { 118, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 0, 0, 0, 97, 0, 0, 0, 2, 102, 105, 110, 100, 0, 16, 0, 0, 0, 84, 101, 115, 116, 67, 111, 108, 108, 101, 99, 116, 105, 111, 110, 52, 0, 3, 102, 105, 108, 116, 101, 114, 0, 5, 0, 0, 0, 0, 2, 36, 100, 98, 0, 8, 0, 0, 0, 84, 101, 115, 116, 68, 98, 50, 0, 3, 108, 115, 105, 100, 0, 30, 0, 0, 0, 5, 105, 100, 0, 16, 0, 0, 0, 4, 131, 62, 100, 234, 152, 197, 69, 158, 132, 179, 177, 163, 226, 139, 75, 233, 0, 0 };
        private static readonly byte[] Req2 = new byte[] { 116, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 0, 0, 0, 95, 0, 0, 0, 2, 102, 105, 110, 100, 0, 15, 0, 0, 0, 84, 101, 115, 116, 67, 111, 108, 108, 101, 99, 116, 105, 111, 110, 0, 3, 102, 105, 108, 116, 101, 114, 0, 5, 0, 0, 0, 0, 2, 36, 100, 98, 0, 7, 0, 0, 0, 84, 101, 115, 116, 68, 98, 0, 3, 108, 115, 105, 100, 0, 30, 0, 0, 0, 5, 105, 100, 0, 16, 0, 0, 0, 4, 240, 238, 24, 182, 212, 193, 68, 157, 154, 174, 170, 95, 96, 34, 122, 59, 0, 0 };


        static async Task Main(string[] args)
        {
            //Test();
            var client = new MongoClient();
            var (connectionInfo, hell) = await client.ConnectAsync(default);
            //var result1 = await client.GetListAsync<Data>(Req1, default);

            for (int i = 0; i < 100; i++)
            {
                var result = await client.GetCursorAsync<GeoIp>(Req2, default).ToListAsync();
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var result = await client.GetCursorAsync<GeoIp>(Req2, default).ToListAsync();
            }
            sw.Stop();
            await client.DisposeAsync();
            Console.WriteLine(sw.Elapsed);

            //var factory = new MongoSessionFactory(new DnsEndPoint("centos0.mshome.net", 27017));
            //var session = await factory.ConnectAsync();
            //var connectionInfo = await session!.SayHelloAsync();
            //await session.DisposeAsync();
            //var root = new BsonDocument();
            //var driverDoc = new BsonDocument();

            //driverDoc.Elements.AddRange(new List<BsonElement>{
            //    BsonElement.Create(driverDoc, "driver", "MongoDB.Client"),
            //    BsonElement.Create(driverDoc, "version", "0.0.0"),
            //});
            //root.Elements.AddRange(new List<BsonElement>
            //{
            //    BsonElement.Create(root, "driver", driverDoc)
            //});

        }
        static void Test()
        {

            ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../ReaderTestCollection.bson");
            //ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../Meteoritelandings.bson");
            var reader = new MongoDBBsonReader(file);
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
