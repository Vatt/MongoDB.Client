using System.Buffers;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class UpdateMessageWriter : IMessageWriter<UpdateMessage>
    {
        public void WriteMessage(UpdateMessage message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            var firstSpan = writer.Reserve(4);

            //writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)PayloadType.Type0);

            UpdateHeader.WriteBson(ref writer, message.UpdateHeader);


            writer.WriteByte((byte)PayloadType.Type1);
            writer.Commit();
            var checkpoint = writer.Written;
            var secondSpan = writer.Reserve(4);
            //writer.WriteInt32(0); // size
            writer.WriteCString("updates");
            UpdateBody.WriteBson(ref writer, message.UpdateBody);

            writer.Commit();
            secondSpan.Write(writer.Written - checkpoint);
            firstSpan.Write(writer.Written);
        }


        private OpMsgFlags CreateFlags(UpdateMessage message)
        {
            return 0;
        }
    }
}
