﻿using System.Buffers;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client.Protocol.Writers
{
    public class FindMessageWriter : IMessageWriter<FindMessage>
    {
        public void WriteMessage(FindMessage message, IBufferWriter<byte> output)
        {
            var writer = new BsonWriter(output);
            var span = writer.Reserve(4);

            //writer.WriteInt32(0); // size
            writer.WriteInt32(message.Header.RequestNumber);
            writer.WriteInt32(0); // responseTo
            writer.WriteInt32((int)message.Header.Opcode);

            writer.WriteInt32((int)CreateFlags(message));

            writer.WriteByte((byte)message.Type);
            /*
            #if DEBUG
                        var buffer = new Utils.ArrayBufferWriter();
                        var writer2 = new BsonWriter(buffer);
                        FindRequest.WriteBson(ref writer2, message.Document);
                        var reader = new BsonReader(buffer.GetSequesnce());
                        BsonDocument.TryParseBson(ref reader, out var bsonDoc);
                        var bson = bsonDoc.ToString();
                        System.Console.WriteLine("Find");
                        System.Console.WriteLine(bson);
            #endif
            */

            FindRequest.WriteBson(ref writer, message.Document);
            writer.Commit();
            span.Write(writer.Written);
        }


        private OpMsgFlags CreateFlags(FindMessage message)
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
