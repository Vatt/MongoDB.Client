using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    public class ReplyMessageReader : IMessageReader<ReplyMessageHeader>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReplyMessageHeader message)
        {
            // The message consists of
            //
            // [4 bytes      ]  [8 bytes ]  [4 bytes     ]  [4 bytes       ]
            // [responseFlags]  [cursorId]  [startingFrom]  [numberReturned]

            if (input.Length < 20)
            {
                message = default;
                return false;
            }

            var reader = new SequenceReader<byte>(input);
            reader.TryReadLittleEndian(out int responseFlags);
            reader.TryReadLittleEndian(out long cursorId);
            reader.TryReadLittleEndian(out int startingFrom);
            reader.TryReadLittleEndian(out int numberReturned);
            message = new ReplyMessageHeader(responseFlags, cursorId, startingFrom, numberReturned);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }
    }
}
