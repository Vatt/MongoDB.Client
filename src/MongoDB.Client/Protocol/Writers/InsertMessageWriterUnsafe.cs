using System.Buffers;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    internal class InsertMessageWriterUnsafe<T> : IMessageWriter<InsertMessage<T>> 
        where T: IBsonSerializer<T>
    {
        public unsafe void WriteMessage(InsertMessage<T> message, IBufferWriter<byte> output)
        {
            var firstSpan = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)PayloadType.Type0);

//#if DEBUG
//            var buffer = new Utils.ArrayBufferWriter();
//            var writer2 = new BsonWriter(buffer);
//            InsertHeader.WriteBson(ref writer2, message.InsertHeader);
//            var reader = new BsonReader(buffer.GetSequesnce());
//            BsonDocument.TryParseBson(ref reader, out var bsonDoc);
//            var bson = bsonDoc.ToString();
//            System.Console.WriteLine("Insert");
//            System.Console.WriteLine(bson);
//#endif

            InsertHeader.WriteBson(ref writer, message.InsertHeader);


            writer.WriteByte((byte)PayloadType.Type1);
            writer.Commit();
            var checkpoint = writer.Written;
            var secondSpan = output.GetSpan();
            writer.WriteInt32(0); // size
            writer.WriteCString("documents");

            foreach (var item in message.Items)
            {
                T.WriteBson(ref writer, item);
            }

            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(secondSpan, writer.Written - checkpoint);
            BinaryPrimitives.WriteInt32LittleEndian(firstSpan, writer.Written);
        }
        private OpMsgFlags CreateFlags(InsertMessage<T> message)
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
    }
}
