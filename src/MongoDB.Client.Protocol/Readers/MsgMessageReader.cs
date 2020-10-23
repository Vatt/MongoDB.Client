using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;


namespace MongoDB.Client.Protocol.Readers
{
    public class MsgMessageReader : IMessageReader<MsgMessageHeader>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out MsgMessageHeader message)
        {
            // The message consists of
            //
            // [4 bytes   ]
            // [OpMsgFlags]

            if (input.Length < 4)
            {
                message = default;
                return false;
            }

            var reader = new SequenceReader<byte>(input);
            reader.TryReadLittleEndian(out int msgFlags);

            message = new MsgMessageHeader(msgFlags);
            consumed = reader.Position;
            examined = reader.Position;
            return true;
        }



    }
}
