using System.Buffers;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class GetMoreMessageWriter : IMessageWriter<GetMoreMessage>
    {
        public void WriteMessage(GetMoreMessage message, IBufferWriter<byte> output)
        {
            var span = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Opcode);


            writer.WriteInt32(0); // 0 - reserved for future use
            writer.WriteCString(message.FullCollectionName);
            writer.WriteInt32(message.NumberToReturn);
            writer.WriteInt64(message.CursorId);
            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(span, writer.Written);
        }
    }
}
