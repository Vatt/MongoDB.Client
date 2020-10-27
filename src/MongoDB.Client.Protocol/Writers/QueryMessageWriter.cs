using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Protocol.Common;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Buffers.Binary;

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
            writer.WriteInt32((int)Opcode.Query); 
            writer.WriteInt32((int)BuildQueryFlags(message));
            writer.WriteCString(message.Database);
            writer.WriteInt32(0);  // message.Skip
            writer.WriteInt32(-1);  // message.BatchSize
            writer.WriteDocument(message.Document);
            // WriteOptionalFields(binaryWriter, message.Fields);
            writer.Commit();
            BinaryPrimitives.WriteInt32LittleEndian(span, writer.Writen);
        }



        private QueryFlags BuildQueryFlags(QueryMessage message)
        {
            var flags = QueryFlags.None;


            flags |= QueryFlags.SlaveOk;


//            if (message.NoCursorTimeout)
//            {
//                flags |= QueryFlags.NoCursorTimeout;
//            }
//#pragma warning disable 618
//            if (message.OplogReplay)
//            {
//                flags |= QueryFlags.OplogReplay;
//            }
//#pragma warning restore 618
//            if (message.PartialOk)
//            {
//                flags |= QueryFlags.Partial;
//            }
//            if (message.SlaveOk)
//            {
//                flags |= QueryFlags.SlaveOk;
//            }
//            if (message.TailableCursor)
//            {
//                flags |= QueryFlags.TailableCursor;
//            }
//            if (message.AwaitData)
//            {
//                flags |= QueryFlags.AwaitData;
//            }
            return flags;
        }


        [Flags]
        private enum QueryFlags
        {
            None = 0,
            TailableCursor = 2,
            SlaveOk = 4,
            [Obsolete("OplogReplay is ignored by server versions 4.4.0 and newer.")]
            OplogReplay = 8,
            NoCursorTimeout = 16,
            AwaitData = 32,
            Exhaust = 64,
            Partial = 128
        }
    }
}
