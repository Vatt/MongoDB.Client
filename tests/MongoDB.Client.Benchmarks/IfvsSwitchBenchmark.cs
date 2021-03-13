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
        private SwitchGroupNamesModel _switchGroup = SwitchGroupNamesModel.Create();
        private SwitchNonGroupNamesModel _switchNonGroup = SwitchNonGroupNamesModel.Create();
        private ContextTreeGroupNamesModel _contextTreeGroup = ContextTreeGroupNamesModel.Create();
        private ArrayBufferWriter _ifBuffer;
        private ArrayBufferWriter _switchBuffer;
        private ArrayBufferWriter _switchGroupBuffer;
        private ArrayBufferWriter _switchNonGroupBuffer;
        private ArrayBufferWriter _contextTreeGroupBuffer;
        
        [GlobalSetup]
        public void Setup()
        {
            _ifShort = IfShortNamesModel.Create();
            _switchShort = SwitchShortNamesModel.Create();
            _ifBuffer = new ArrayBufferWriter(1024 * 1024);
            _switchBuffer = new ArrayBufferWriter(1024 * 1024);
            _switchGroupBuffer = new ArrayBufferWriter(1024 * 1024);
            _switchNonGroupBuffer = new ArrayBufferWriter(1024 * 1024);
            _contextTreeGroupBuffer = new ArrayBufferWriter(1024 * 1024);
            var writerIf = new BsonWriter(_ifBuffer);
            var writerSwitch = new BsonWriter(_switchBuffer);
            var writerSwitchGroup = new BsonWriter(_switchGroupBuffer);
            var writerSwitchNonGroup = new BsonWriter(_switchNonGroupBuffer);
            var writerContextTreeGroup = new BsonWriter(_contextTreeGroupBuffer);
            IfShortNamesModel.WriteBson(ref writerIf, _ifShort);
            SwitchShortNamesModel.WriteBson(ref writerSwitch, _switchShort);
            SwitchGroupNamesModel.WriteBson(ref writerSwitchGroup, _switchGroup);
            SwitchNonGroupNamesModel.WriteBson(ref writerSwitchNonGroup, _switchNonGroup);
            ContextTreeGroupNamesModel.WriteBson(ref writerContextTreeGroup, _contextTreeGroup);
        }
        [Benchmark]
        public IfShortNamesModel ReadIf()
        {
            var reader = new BsonReader(_ifBuffer.WrittenMemory);
            IfShortNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public SwitchShortNamesModel ReadSwitch()
        {
            var reader = new BsonReader(_switchBuffer.WrittenMemory);
            SwitchShortNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public SwitchGroupNamesModel ReadSwitchGroup()
        {
            var reader = new BsonReader(_switchGroupBuffer.WrittenMemory);
            SwitchGroupNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public SwitchNonGroupNamesModel ReadSwitchNonGroup()
        {
            var reader = new BsonReader(_switchNonGroupBuffer.WrittenMemory);
            SwitchNonGroupNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public ContextTreeGroupNamesModel ReadContextTreeGroup()
        {
            var reader = new BsonReader(_contextTreeGroupBuffer.WrittenMemory);
            ContextTreeGroupNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

    }
}