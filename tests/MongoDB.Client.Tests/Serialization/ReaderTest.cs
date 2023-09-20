using System.Buffers;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
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
            try
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
            catch (Exception)
            {
                throw;
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
