using System;
using System.Buffers;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using Sprache;
using Xunit;

namespace MongoDB.Client.Tests.Serialization;

public class ReaderTest
{
    [Fact]
    public void Deserialization()
    {
        var doc = new BsonDocument
            {
                { "int", 42},
                { "bool", true},
                { "string1", "string"},
                { "string2", ""},
                { "string3", default(string)},
                { "objectId", new BsonObjectId("5f987814bf344ec7cc57294b")},
                {"array", new  BsonArray { "item1", default(string), 42, true } },
                { "inner", new BsonDocument {
                    {"innerString", "inner string" }
                } }
            };
        var buffer = new ArrayBufferWriter(1024 * 1024);
        var writer = new BsonWriter(buffer);
        BsonDocument.WriteBson(ref writer, doc);

        for (var i = 0; i < buffer.WrittenMemory.Length; i++)
        {
            var mem1 = buffer.WrittenMemory.Slice(0, i);
            var mem2 = buffer.WrittenMemory.Slice(i);
            var segment = new MemorySegment(mem1, mem2);
            var seq = new ReadOnlySequence<byte>(segment, 0, segment.Next, segment.Next.Memory.Length);
            var reader = new BsonReader(seq);
            var result = BsonDocument.TryParseBson(ref reader, out var message);
            Assert.True(result);
            Assert.Equal(doc, message);
        }
    }

    [Fact]
    public void Writer()
    {
        var doc = new BsonDocument
            {
                { "int", 42},
                { "bool", true},
                { "string1", "string"},
                { "string2", ""},
                { "string3", default(string)},
                { "objectId", new BsonObjectId("5f987814bf344ec7cc57294b")},
                {"array", new  BsonArray { "item1", default(string), 42, true } },
                { "inner", new BsonDocument {
                    {"innerString", "inner string" }
                } }
            };
        var length = GetDataLength(doc);

        for (var i = 0; i < length; i++)
        {
            BsonDocument message;
            if (i == 6)
            {
                var a = 1;
            }
            try
            {
                var segBuffer = new SegmentedBufferWriter(i, length * 2);
                var writer = new BsonWriter(segBuffer);
                BsonDocument.WriteBson(ref writer, doc);

                var reader = new BsonReader(segBuffer.GetSequence());
                var result = BsonDocument.TryParseBson(ref reader, out message);
                Assert.True(result);
                Assert.Equal(doc, message);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }

    private int GetDataLength(BsonDocument doc)
    {
        var buffer = new ArrayBufferWriter(1024 * 1024);
        var writer = new BsonWriter(buffer);
        BsonDocument.WriteBson(ref writer, doc);
        return buffer.WrittenMemory.Length;
    }

    public class SegmentedBufferWriter : IBufferWriter<byte>
    {
        private SegmentBuffer _buffer1;
        private SegmentBuffer _buffer2;
        private SegmentBuffer _currentBuffer;

        public SegmentedBufferWriter(int offset, int bufferSize = 1024 * 1024)
        {
            _currentBuffer = _buffer1 = new SegmentBuffer(new byte[offset]);
            _buffer2 = new SegmentBuffer(new byte[bufferSize - offset]);
        }

        public void Reset()
        {
            _buffer1.Reset();
            _buffer2.Reset();
        }

        public void Advance(int count)
        {
            _currentBuffer.Advance(count);
        }
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (_currentBuffer.Memory.IsEmpty)
            {
                _currentBuffer = _buffer2;
            }
            return _currentBuffer.Memory;
        }
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (_currentBuffer.Span.IsEmpty)
            {
                _currentBuffer = _buffer2;
            }
            return _currentBuffer.Span;
        }

        public ReadOnlySequence<byte> GetSequence()
        {
            var mem1 = _buffer1.WrittenMemory;
            var mem2 = _buffer2.WrittenMemory;
            var segment = new MemorySegment(mem1, mem2);
            return new ReadOnlySequence<byte>(segment, 0, segment.Next, segment.Next.Memory.Length);
        }

        private record SegmentBuffer(byte[] Buffer)
        {
            public int Position { get; private set; }

            public Memory<byte> Memory => Buffer.AsMemory(Position);
            public Span<byte> Span => Buffer.AsSpan(Position);

            public Memory<byte> WrittenMemory => Buffer.AsMemory(0, Position);

            public void Reset()
            {
                Position = 0;
            }
            public void Advance(int count)
            {
                Position += count;
            }
        }
    }

    public class MemorySegment : ReadOnlySequenceSegment<byte>
    {
        public MemorySegment(ReadOnlyMemory<byte> memory1, ReadOnlyMemory<byte> memory2)
        {
            Memory = memory1;
            if (memory2.IsEmpty == false)
            {
                Next = new MemorySegment(memory2)
                {
                    RunningIndex = Memory.Length
                };
            }
        }

        public MemorySegment(ReadOnlyMemory<byte> memory)
            : this(memory, default)
        {
        }
    }
}
