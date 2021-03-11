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
        private ArrayBufferWriter _readIfBuffer;
        private ArrayBufferWriter _readSwitchBuffer;
        private ArrayBufferWriter _readSwitchGroupBuffer;
        private ArrayBufferWriter _readSwitchNonGroupBuffer;
        
        [GlobalSetup]
        public void Setup()
        {
            _ifShort = IfShortNamesModel.Create();
            _switchShort = SwitchShortNamesModel.Create();
            _readIfBuffer = new ArrayBufferWriter(1024 * 1024);
            _readSwitchBuffer = new ArrayBufferWriter(1024 * 1024);
            _readSwitchGroupBuffer = new ArrayBufferWriter(1024 * 1024);
            _readSwitchNonGroupBuffer = new ArrayBufferWriter(1024 * 1024);
            var writerIf = new BsonWriter(_readIfBuffer);
            var writerSwitch = new BsonWriter(_readSwitchBuffer);
            IfShortNamesModel.WriteBson(ref writerIf, _ifShort);
            SwitchShortNamesModel.WriteBson(ref writerSwitch, _switchShort);
            var writerSwitchGroup = new BsonWriter(_readIfBuffer);
            var writerSwitchNonGroup = new BsonWriter(_readSwitchBuffer);
            SwitchGroupNamesModel.WriteBson(ref writerSwitchNonGroup, _switchGroup);
            SwitchNonGroupNamesModel.WriteBson(ref writerSwitchNonGroup, _switchNonGroup);
        }
        [Benchmark]
        public IfShortNamesModel ReadIf()
        {
            var reader = new BsonReader(_readIfBuffer.WrittenMemory);
            IfShortNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public SwitchShortNamesModel ReadSwitch()
        {
            var reader = new BsonReader(_readSwitchBuffer.WrittenMemory);
            SwitchShortNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }
        [Benchmark]
        public SwitchGroupNamesModel ReadSwitchGroup()
        {
            var reader = new BsonReader(_readIfBuffer.WrittenMemory);
            SwitchGroupNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

        [Benchmark]
        public SwitchNonGroupNamesModel ReadSwitchNonGroup()
        {
            var reader = new BsonReader(_readSwitchBuffer.WrittenMemory);
            SwitchNonGroupNamesModel.TryParseBson(ref reader, out var parsedItem);
            return parsedItem;
        }

    }
}