using BenchmarkDotNet.Attributes;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Client.Bson.Document;
using BsonWriter = MongoDB.Client.Bson.Writer.BsonWriter;

namespace MongoDB.Client.Benchmarks.Serialization
{
    [MemoryDiagnoser]
    public class SerializationBenchmarks
    {
        private static readonly byte[] ArrayBuffer = new byte[4096];
        private BsonDocument _document;
        private MongoDB.Bson.BsonDocument _bsonDocument;

        [GlobalSetup]
        public void GlobalInit()
        {
            _document = new BsonDocument
            {
                {"int", 42},
                {"bool", true},
                {"string1", "string"},
                {"string2", ""},
                {"array", new BsonArray {"item1", 42, true}},
                {
                    "inner", new BsonDocument
                    {
                        {"innerString", "inner string"}
                    }
                }
            };
            _bsonDocument = new MongoDB.Bson.BsonDocument
            {
                {"int", 42},
                {"bool", true},
                {"string1", "string"},
                {"string2", ""},
                {"array", new MongoDB.Bson.BsonArray {"item1", 42, true}},
                {
                    "inner", new MongoDB.Bson.BsonDocument
                    {
                        {"innerString", "inner string"}
                    }
                }
            };
        }

        [Benchmark]
        public int BsonSerialization()
        {
            var buffer = new TestBuffer(ArrayBuffer);
            var writer = new BsonWriter(buffer);
            writer.WriteDocument(_document);
            return buffer.Written;
        }

        [Benchmark]
        public int OldBsonSerialization()
        {
            using (var stream = new MemoryStream(ArrayBuffer))
            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, _bsonDocument);
                return (int)stream.Position;
            }
        }
    }
}