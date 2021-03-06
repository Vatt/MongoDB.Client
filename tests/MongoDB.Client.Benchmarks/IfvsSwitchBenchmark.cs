using BenchmarkDotNet.Attributes;
using MongoDB.Client.Benchmarks.Serialization;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Tests.Models;

namespace MongoDB.Client.Benchmarks
{
    [MemoryDiagnoser]
    public class IfvsSwitchBenchmark
    {
        private IfShortNamesModel _ifShort = IfShortNamesModel.Create();
        private SwitchShortNamesModel _switchShort = SwitchShortNamesModel.Create();
        private ArrayBufferWriter _writeBuffer;
        private ArrayBufferWriter _readBuffer;
        
        [GlobalSetup]
        public void Setup()
        {
            _ifShort = IfShortNamesModel.Create();
            _switchShort = SwitchShortNamesModel.Create();
            _writeBuffer = new ArrayBufferWriter(1024 * 1024);
            _readBuffer = new ArrayBufferWriter(1024 * 1024);
        }
        [Benchmark]
        public IfShortNamesModel ReadIf()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            IfShortNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public void WriteIf()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            IfShortNamesModel.WriteBson(ref writer, _ifShort);
        }
        [Benchmark]
        public SwitchShortNamesModel ReadSwitch()
        {
            var reader = new BsonReader(_readBuffer.WrittenMemory);
            SwitchShortNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public void WriteSwitch()
        {
            _writeBuffer.Reset();
            var writer = new BsonWriter(_writeBuffer);
            SwitchShortNamesModel.WriteBson(ref writer, _switchShort);
        }
    }
}