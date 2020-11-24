using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using MongoDB.Client.Benchmarks.Serialization;
using MongoDB.Client.Benchmarks.Serialization.Models;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class SerializationBenchmark
    {
        private RootDocument _document;
        private IGenericBsonSerializer<RootDocument> _serializer;
        private ArrayBufferWriter _writeBuffer;
        private ArrayBufferWriter _readBuffer;

        [GlobalSetup]
        public void Setup()
        {
            var seeder = new DatabaseSeeder();
            _document = seeder.GenerateSeed().First();
            SerializersMap.TryGetSerializer<RootDocument>(out _serializer);
            _writeBuffer = new ArrayBufferWriter(1024 * 1024);
            _readBuffer = new ArrayBufferWriter(1024 * 1024);
            var writer = new BsonWriter(_readBuffer);
            _serializer.Write(ref writer, _document);
        }


        [Benchmark]
        public RootDocument Read()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            _serializer.TryParse(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public void Write()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            _serializer.Write(ref writer, _document);
        }
    }
}
