using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client;

namespace MongoDB.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Test();
            var client = new MongoClient();
            var connectionInfo = await client.ConnectAsync(default);

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
            IBsonSerializable serializator = new MongoDB.Client.Bson.Serialization.Generated.DocumentObjectGeneratedSerializator();

            serializator.TryParse(ref reader, out var doc);
            //reader.TryParseDocument(null, out var document);
            //reader.TryParseDocument(null, out var document1);
            //reader.TryParseDocument(null, out var document2);
            //reader.TryParseDocument(null, out var document3);
            //reader.TryParseDocument(null, out var document4);
            //reader.TryParseDocument(null, out var document5);
            
        }
    }
}
