using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System.Buffers;
using System.Buffers.Binary;

namespace MongoDB.Client.Protocol.Writers
{
    public class CreateCollectionMessageWriter : IMessageWriter<CreateCollectionMessage>
    {
        public void WriteMessage(CreateCollectionMessage message, IBufferWriter<byte> output)
        {
            var span = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)PayloadType.Type0);

            CreateCollectionHeader.WriteBson(ref writer, message.CreateCollectionHeader);

            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(span, writer.Written);
        }


        private OpMsgFlags CreateFlags(CreateCollectionMessage message)
        {
            return 0;
        }
    }
}