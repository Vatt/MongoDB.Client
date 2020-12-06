using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Client.Benchmarks.Serialization;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using BsonReader = MongoDB.Client.Bson.Reader.BsonReader;
using BsonWriter = MongoDB.Client.Bson.Writer.BsonWriter;
using GenericDocument = MongoDB.Client.Benchmarks.Serialization.Models.GenericDocument<double, string, MongoDB.Client.Bson.Document.BsonDocument, MongoDB.Client.Bson.Document.BsonObjectId, int, long, 
                                                                                        System.DateTimeOffset, System.Guid, 
                                                                                        MongoDB.Client.Benchmarks.Serialization.Models.AnotherGenericModel<int>, 
                                                                                        MongoDB.Client.Benchmarks.Serialization.Models.AnotherGenericModel<string>>;
namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class GenericsBenchmark
    {
        private NonGenericDocument _document;
        private GenericDocument _genericDocument;
        private ArrayBufferWriter _writeBuffer;
        private ArrayBufferWriter _readBuffer;
        private byte[] _documentBson;
        private byte[] _buffer = new byte[1024 * 1024];
        [GlobalSetup]
        public void Setup()
        {
            var seeder = new GenericDatabaseSeeder();
            _document = seeder.GenerateSeed().First();
            _genericDocument = seeder.GenerateGenericSeed().First();
            _writeBuffer = new ArrayBufferWriter(1024 * 1024);
            _readBuffer = new ArrayBufferWriter(1024 * 1024);
            var writer = new BsonWriter(_readBuffer);
            NonGenericDocument.Write(ref writer, _document);
            _documentBson = _document.ToBson();
        }
        [Benchmark]
        public NonGenericDocument ReadNonGeneric()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            NonGenericDocument.TryParse(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public void WriteNonGeneric()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            NonGenericDocument.Write(ref writer, _document);
        }

        [Benchmark]
        public GenericDocument ReadGeneric()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            GenericDocument.TryParse(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public void WriteGeneric()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            GenericDocument.Write(ref writer, _genericDocument);
        }
    }
}