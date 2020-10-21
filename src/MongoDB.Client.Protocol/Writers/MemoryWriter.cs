using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;

namespace MongoDB.Client.Protocol.Writers
{
    public class MemoryWriter : IMessageWriter<ReadOnlyMemory<byte>>
    {
        public void WriteMessage(ReadOnlyMemory<byte> message, IBufferWriter<byte> output)
        {
            output.Write(message.Span);
        }
    }
}
