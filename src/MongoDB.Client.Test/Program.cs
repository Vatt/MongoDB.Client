using MongoDB.Client;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MongoDB.Test
{
    class Program
    {
        private static ReadOnlyMemory<byte> Req1 = new byte[] { 141, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 1, 0, 0, 120, 0, 0, 0, 16, 105, 115, 77, 97, 115, 116, 101, 114, 0, 1, 0, 0, 0, 3, 116, 111, 112, 111, 108, 111, 103, 121, 86, 101, 114, 115, 105, 111, 110, 0, 45, 0, 0, 0, 7, 112, 114, 111, 99, 101, 115, 115, 73, 100, 0, 95, 144, 11, 88, 84, 53, 76, 175, 109, 171, 30, 186, 18, 99, 111, 117, 110, 116, 101, 114, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 18, 109, 97, 120, 65, 119, 97, 105, 116, 84, 105, 109, 101, 77, 83, 0, 16, 39, 0, 0, 0, 0, 0, 0, 2, 36, 100, 98, 0, 6, 0, 0, 0, 97, 100, 109, 105, 110, 0, 0 };
        private static ReadOnlyMemory<byte> Req2 = new byte[] { 118, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 0, 0, 0, 97, 0, 0, 0, 2, 102, 105, 110, 100, 0, 16, 0, 0, 0, 84, 101, 115, 116, 67, 111, 108, 108, 101, 99, 116, 105, 111, 110, 52, 0, 3, 102, 105, 108, 116, 101, 114, 0, 5, 0, 0, 0, 0, 2, 36, 100, 98, 0, 8, 0, 0, 0, 84, 101, 115, 116, 68, 98, 50, 0, 3, 108, 115, 105, 100, 0, 30, 0, 0, 0, 5, 105, 100, 0, 16, 0, 0, 0, 4, 131, 62, 100, 234, 152, 197, 69, 158, 132, 179, 177, 163, 226, 139, 75, 233, 0, 0 };

        static async Task Main(string[] args)
        {
            //Test();
            var client = new MongoClient();
            var connectionInfo = await client.ConnectAsync(default);
            var result1 = await client.GetListAsync<BsonDocument>(Req1, default);
            var result2 = await client.GetListAsync<BsonDocument>(Req2, default);


            Console.WriteLine();


            //var factory = new MongoDBSessionFactory(new DnsEndPoint("centos0.mshome.net", 27017));
            //var session = await factory.ConnectAsync();
            //var connectionInfo = await session!.SayHelloAsync();
            //await session.DisposeAsync();
            //var factory = new NetworkConnectionFactory();
            //var connection = await factory.ConnectAsync(new DnsEndPoint("centos0.mshome.net", 27017));
            //var seq = await connection.Pipe.Input.ReadAsync();
            var root = new BsonDocument();
            var driverDoc = new BsonDocument();

            driverDoc.Elements.AddRange(new List<BsonElement>{
                BsonElement.Create(driverDoc, "driver", "MongoDB.Client"),
                BsonElement.Create(driverDoc, "version", "0.0.0"),
            });
            root.Elements.AddRange(new List<BsonElement>
            {
                BsonElement.Create(root, "driver", driverDoc)
            });

        }
        static void Test()
        {

            ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../ReaderTestCollection.bson");
            //ReadOnlyMemory<byte> file = File.ReadAllBytes("../../../Meteoritelandings.bson");
            var reader = new MongoDBBsonReader(file);
            //IBsonSerializable serializator = new MongoDB.Client.Test.Generated.NasaMeteoriteLandingGeneratedSerializator();
            //IBsonSerializable serializator = new MongoDB.Client.Bson.Serialization.Generated.MongoDBConnectionInfoGeneratedSerializator();

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
