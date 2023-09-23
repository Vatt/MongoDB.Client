using System.Buffers;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class TransactionMessageWriter : IMessageWriter<TransactionMessage>
    {
        public void WriteMessage(TransactionMessage message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            var span = writer.Reserve(4);

            //writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)PayloadType.Type0);
            TransactionRequest.WriteBson(ref writer, message.Request);
            writer.Commit();
            span.Write(writer.Written);
        }


        private OpMsgFlags CreateFlags(TransactionMessage message)
        {
            return 0;
        }
    }
}
