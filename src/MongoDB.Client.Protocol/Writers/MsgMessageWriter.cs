using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Buffers.Binary;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class MsgMessageWriter : IMessageWriter<MsgMessage>
    {
        public void WriteMessage(MsgMessage message, IBufferWriter<byte> output)
        {
            var span = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.RequestNumber); 
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Opcode); 
            writer.WriteInt32((int)CreateFlags(message));
            writer.WriteByte((byte)message.Type);
            writer.WriteDocument(message.Document);
            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(span, writer.Writen);
        }



        private OpMsgFlags CreateFlags(MsgMessage message)
        {
            var flags = (OpMsgFlags)0;
            if (message.MoreToCome)
            {
                flags |= OpMsgFlags.MoreToCome;
            }
            if (message.ExhaustAllowed)
            {
                flags |= OpMsgFlags.ExhaustAllowed;
            }
            return flags;
        }

        [Flags]
        internal enum OpMsgFlags
        {
            ChecksumPresent = 1 << 0,
            MoreToCome = 1 << 1,
            ExhaustAllowed = 1 << 16,
            All = ChecksumPresent | MoreToCome | ExhaustAllowed
        }
    }
}
