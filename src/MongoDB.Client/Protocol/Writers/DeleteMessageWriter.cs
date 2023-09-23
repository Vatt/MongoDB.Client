using System.Buffers;
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
            var writer = new BsonWriter(output);
            var firstSpan = writer.Reserve(4);


            //writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)PayloadType.Type0);

            DeleteHeader.WriteBson(ref writer, message.DeleteHeader);


            writer.WriteByte((byte)PayloadType.Type1);
            writer.Commit();
            var checkpoint = writer.Written;
            var secondSpan = writer.Reserve(4);
            //writer.WriteInt32(0); // size
            writer.WriteCString("deletes");

            DeleteBody.WriteBson(ref writer, message.DeleteBody);

            writer.Commit();
            secondSpan.Write(writer.Written - checkpoint);
            firstSpan.Write(writer.Written);
        }


        private OpMsgFlags CreateFlags(DeleteMessage message)
        {
            return 0;
        }
    }
}
