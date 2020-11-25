﻿using System.IO;
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
    public class SerializationBenchmark
    {
        private RootDocument _document;
        private IGenericBsonSerializer<RootDocument> _serializer;
        private ArrayBufferWriter _writeBuffer;
        private ArrayBufferWriter _readBuffer;
        private byte[] _documentBson;
        private byte[] _buffer = new byte[1024 * 1024];

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
            _documentBson = _document.ToBson();
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

        [Benchmark]
        public RootDocument ReadOld()
        {
            return BsonSerializer.Deserialize<RootDocument>(_documentBson);
        }

        [Benchmark]
        public void WriteOld()
        {
            using (var stream = new MemoryStream(_buffer))
            using (var writer = new BsonBinaryWriter(stream))
            {
                BsonSerializer.Serialize(writer, _document);
            }
        }
    }
}
