﻿using System.Buffers;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class DropCollectionMessageWriter : IMessageWriter<DropCollectionMessage>
    {
        public void WriteMessage(DropCollectionMessage message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            var span = writer.Reserve(4);

            //writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)PayloadType.Type0);

            DropCollectionHeader.WriteBson(ref writer, message.DropCollectionHeader);

            writer.Commit();
            span.Write(writer.Written);
        }


        private OpMsgFlags CreateFlags(DropCollectionMessage message)
        {
            return 0;
        }
    }
}
