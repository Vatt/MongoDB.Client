using System.Linq;
using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization;
using MongoDB.Client.Benchmarks.Serialization.Models;
using BsonReader = MongoDB.Client.Bson.Reader.BsonReader;
using BsonWriter = MongoDB.Client.Bson.Writer.BsonWriter;
using GenericDocument = MongoDB.Client.Tests.Models.GenericDocument<double, string, MongoDB.Client.Bson.Document.BsonDocument, MongoDB.Client.Bson.Document.BsonObjectId, int, long,
                                                                                        System.DateTimeOffset, System.Guid, MongoDB.Client.Tests.Models.AnotherGenericModel<int>, MongoDB.Client.Tests.Models.AnotherGenericModel<string>>;
using NonGenericDocument = MongoDB.Client.Tests.Models.NonGenericDocument;
using SmallGenericDocument = MongoDB.Client.Tests.Models.SmallGenericDocument<MongoDB.Client.Tests.Models.AnotherGenericModel<int>, MongoDB.Client.Tests.Models.AnotherGenericModel<string>>;
using SmallNonGenericDocument = MongoDB.Client.Tests.Models.SmallNonGenericDocument;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class GenericsBenchmark
    {
        private NonGenericDocument _document;
        private GenericDocument _genericDocument;
        private SmallNonGenericDocument _smallDocument;
        private SmallGenericDocument _smallGenericDocument;
        private ArrayBufferWriter _writeBuffer;
        private ArrayBufferWriter _readBuffer;

        [GlobalSetup]
        public void Setup()
        {
            var seeder = new GenericDatabaseSeeder();
            _document = seeder.GenerateSeed(/*2000*/).First();
            _genericDocument = seeder.GenerateGenericSeed(/*2000*/).First();
            _smallDocument = seeder.GenerateSmallSeed(/*2000*/).First();
            _smallGenericDocument = seeder.GenerateSmallGenericSeed(/*2000*/).First();
            _writeBuffer = new ArrayBufferWriter(1024 * 1024);
            _readBuffer = new ArrayBufferWriter(1024 * 1024);
            var writer = new BsonWriter(_readBuffer);
            NonGenericDocument.WriteBson(ref writer, _document);
        }
        [Benchmark]
        public NonGenericDocument ReadNonGeneric()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            NonGenericDocument.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public void WriteNonGeneric()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            NonGenericDocument.WriteBson(ref writer, _document);
        }
        [Benchmark]
        public GenericDocument ReadGeneric()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            GenericDocument.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public void WriteGeneric()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            GenericDocument.WriteBson(ref writer, _genericDocument);
        }



        [Benchmark]
        public SmallNonGenericDocument ReadSmallNonGeneric()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            SmallNonGenericDocument.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public void WriteSmallNonGeneric()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            SmallNonGenericDocument.WriteBson(ref writer, _smallDocument);
        }
        [Benchmark]
        public SmallGenericDocument ReadSmallGeneric()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            SmallGenericDocument.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public void WriteSmallGeneric()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            SmallGenericDocument.WriteBson(ref writer, _smallGenericDocument);
        }


    }
}