using AMQP.Client.RabbitMQ.Protocol.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Protocol
{
    public class MessageHeaderReader : IMessageReader<MessageHeader>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out MessageHeader message)
        {
            message = default;
            if (input.Length < sizeof(int) * 4)
            {
                return false;
            }
            var reader = new SequenceReader<byte>(input);
            reader.TryReadLittleEndian(out int messageLength);
            reader.TryReadLittleEndian(out int requestId);
            reader.TryReadLittleEndian(out int responseTo);
            reader.TryReadLittleEndian(out int opcode);
            message = new MessageHeader(messageLength, requestId, responseTo, opcode);
            return true;
        }
    }
}
