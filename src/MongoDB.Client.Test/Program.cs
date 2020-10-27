using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Readers;
using MongoDB.Client.Writers;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MongoDB.Client.Test
{
    class Program
    {
        private static readonly byte[] NotEmptyCollection = new byte[] { 116, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 0, 0, 0, 95, 0, 0, 0, 2, 102, 105, 110, 100, 0, 15, 0, 0, 0, 84, 101, 115, 116, 67, 111, 108, 108, 101, 99, 116, 105, 111, 110, 0, 3, 102, 105, 108, 116, 101, 114, 0, 5, 0, 0, 0, 0, 2, 36, 100, 98, 0, 7, 0, 0, 0, 84, 101, 115, 116, 68, 98, 0, 3, 108, 115, 105, 100, 0, 30, 0, 0, 0, 5, 105, 100, 0, 16, 0, 0, 0, 4, 240, 238, 24, 182, 212, 193, 68, 157, 154, 174, 170, 95, 96, 34, 122, 59, 0, 0 };
        private static readonly byte[] EmptyCollection = new byte[] { 117, 0, 0, 0, 9, 0, 0, 0, 0, 0, 0, 0, 221, 7, 0, 0, 0, 0, 0, 0, 0, 96, 0, 0, 0, 2, 102, 105, 110, 100, 0, 16, 0, 0, 0, 84, 101, 115, 116, 67, 111, 108, 108, 101, 99, 116, 105, 111, 110, 50, 0, 3, 102, 105, 108, 116, 101, 114, 0, 5, 0, 0, 0, 0, 2, 36, 100, 98, 0, 7, 0, 0, 0, 84, 101, 115, 116, 68, 98, 0, 3, 108, 115, 105, 100, 0, 30, 0, 0, 0, 5, 105, 100, 0, 16, 0, 0, 0, 4, 1, 159, 223, 195, 102, 86, 79, 175, 184, 118, 148, 208, 140, 216, 134, 192, 0, 0 };

        static async Task Main(string[] args)
        {
            await Test2();
            var client = new MongoClient();
            var (connectionInfo, hell) = await client.ConnectAsync(default);
            var result1 = await client.GetCursorAsync<GeoIp>(EmptyCollection, default).ToListAsync();
            var result2 = await client.GetCursorAsync<GeoIp>(NotEmptyCollection, default).ToListAsync();
            var result3 = await client.GetCursorAsync<GeoIp>(EmptyCollection, default).ToListAsync();
            var result4 = await client.GetCursorAsync<GeoIp>(NotEmptyCollection, default).ToListAsync();

            for (int i = 0; i < 100; i++)
            {
                var warmupResult = await client.GetCursorAsync<GeoIp>(NotEmptyCollection, default).ToListAsync();
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var testResult = await client.GetCursorAsync<GeoIp>(NotEmptyCollection, default).ToListAsync();
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


        static async Task Test2()
        {
            var doc = new BsonDocument
            {
                { "int", 42},
                { "bool", true},
                { "string1", "vat hui"},
                { "string2", ""},
                { "string3", default(string)},
                {"array", new  BsonArray { "item1", default(string), 42, true } },
                { "inner", new BsonDocument {
                    {"innerString", "inner vat hui" }
                } }
            };


            var pipe = new Pipe();
            var read = StartReadAsync(pipe.Reader);
            await StartWriteAsync(pipe.Writer, doc);
            var result = await read;

            Console.WriteLine();
        }


        private static async Task<BsonDocument> StartReadAsync(PipeReader input)
        {
            var reader = new ProtocolReader(input);

            var messageReader = new ReplyBodyReader<BsonDocument>(new BsonDocumentSerializer());
            var result = await reader.ReadAsync(messageReader).ConfigureAwait(false);
            reader.Advance();
            return result.Message;
        }


        private static async Task StartWriteAsync(PipeWriter output, BsonDocument message)
        {
            var writer = new ProtocolWriter(output);

            var messageWriter = new ReplyBodyWriter<BsonDocument>(new BsonDocumentSerializer());
            await writer.WriteAsync(messageWriter, message).ConfigureAwait(false);
        }
    }
}
