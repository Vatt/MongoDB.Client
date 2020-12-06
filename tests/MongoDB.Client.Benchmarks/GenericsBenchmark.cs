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

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class GenericsBenchmark
    {
        [GlobalSetup]
        public void Setup()
        {
            var seeder = new GenericDatabaseSeeder();
            var _document = seeder.GenerateSeed().First();
            var _writeBuffer = new ArrayBufferWriter(1024 * 1024);
            var _readBuffer = new ArrayBufferWriter(1024 * 1024);
            var writer = new BsonWriter(_readBuffer);
            NonGenericDocument.Write(ref writer, _document);
            var _documentBson = _document.ToBson();
        }
    }
}