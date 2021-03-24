using System;
using System.Buffers;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class QueryMessageWriter : IMessageWriter<QueryMessage>
    {
        public void WriteMessage(QueryMessage message, IBufferWriter<byte> output)
        {
            var span = output.GetSpan();
            var writer = new BsonWriter(output);

            writer.WriteInt32(0); // size
            writer.WriteInt32(message.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Opcode);


            writer.WriteInt32((int)BuildQueryFlags(message));
            writer.WriteCString(message.FullCollectionName);
            writer.WriteInt32(0);  // message.Skip
            writer.WriteInt32(-1);  // message.BatchSize
            writer.WriteDocument(message.Document);
            // WriteOptionalFields(binaryWriter, message.Fields);
            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(span, writer.Written);
        }



        private QueryFlags BuildQueryFlags(QueryMessage message)
        {
            var flags = QueryFlags.None;


            if (message.NoCursorTimeout)
            {
                flags |= QueryFlags.NoCursorTimeout;
            }
            if (message.PartialOk)
            {
                flags |= QueryFlags.Partial;
            }
            if (message.SlaveOk)
            {
                flags |= QueryFlags.SlaveOk;
            }
            if (message.TailableCursor)
            {
                flags |= QueryFlags.TailableCursor;
            }
            if (message.AwaitData)
            {
                flags |= QueryFlags.AwaitData;
            }
            return flags;
        }


        [Flags]
        private enum QueryFlags
        {
            None = 0,
            TailableCursor = 2,
            SlaveOk = 4,
            NoCursorTimeout = 16,
            AwaitData = 32,
            Exhaust = 64,
            Partial = 128
        }
    }
}
