using MongoDB.Client.Bson.Writer;
using System.Buffers;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class FindMessageWriter : IMessageWriter<FindMessage>
    {
        private static readonly IGenericBsonSerializer<FindRequest> _headerSerializer;

        static FindMessageWriter()
        {
            SerializersMap.TryGetSerializer(out _headerSerializer!);
        }

        public void WriteMessage(FindMessage message, IBufferWriter<byte> output)
        {
            var span = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int) message.Header.Opcode);

            writer.WriteInt32((int) CreateFlags(message));

            writer.WriteByte((byte) message.Type);
            _headerSerializer.Write(ref writer, message.Document);
            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(span, writer.Written);
        }


        private OpMsgFlags CreateFlags(FindMessage message)
        {
            var flags = (OpMsgFlags) 0;
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
    }
}