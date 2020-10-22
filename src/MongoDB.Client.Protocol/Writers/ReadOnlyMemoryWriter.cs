using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;


namespace MongoDB.Client.Protocol.Writers
{
    public class ReadOnlyMemoryWriter : IMessageWriter<ReadOnlyMemory<byte>>
    {
        public void WriteMessage(ReadOnlyMemory<byte> message, IBufferWriter<byte> output)
        {
            output.Write(message.Span);
        }
    }
}
