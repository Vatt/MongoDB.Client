using System.Buffers;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class DeleteMessageWriter : IMessageWriter<DeleteMessage>
    {
        public void WriteMessage(DeleteMessage message, IBufferWriter<byte> output)
        {
            var firstSpan = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int) message.Header.Opcode);

            writer.WriteInt32((int) CreateFlags(message));

            writer.WriteByte((byte) PayloadType.Type0);

            DeleteHeader.Write(ref writer, message.DeleteHeader);


            writer.WriteByte((byte) PayloadType.Type1);
            writer.Commit();
            var checkpoint = writer.Written;
            var secondSpan = output.GetSpan();
            writer.WriteInt32(0); // size
            writer.WriteCString("deletes");

            DeleteBody.Write(ref writer, message.DeleteBody);

            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(secondSpan, writer.Written - checkpoint);
            BinaryPrimitives.WriteInt32LittleEndian(firstSpan, writer.Written);
        }


        private OpMsgFlags CreateFlags(DeleteMessage message)
        {
            return 0;
        }
    }
}