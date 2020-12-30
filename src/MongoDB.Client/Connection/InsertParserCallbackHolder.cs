using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Protocol.Writers;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Connection
{
    internal static class InsertParserCallbackHolder<T>
    {
        private static unsafe readonly delegate*<ref BsonWriter, in T, void> WriterFnPtr;
        public static Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>>? Parser;
        private static readonly IMessageWriter<InsertMessage<T>> InsertMessageWriter;
        private static readonly InsertMsgType0BodyReader InsertBodyReader = new InsertMsgType0BodyReader();
        static unsafe InsertParserCallbackHolder()
        {
            SerializersMap.TryGetSerializer<T>(out var serializer);
            WriterFnPtr = SerializerFnPtrProvider<T>.WriteFnPtr;
            InsertMessageWriter = WriterFnPtr != null ? new InsertMessageWriterUnsafe<T>() : new InsertMessageWriter<T>(serializer);
        }
        public static async ValueTask<IParserResult> InsertParseAsync(ProtocolReader reader, MongoResponseMessage response)
        {
            switch (response)
            {
                case ResponseMsgMessage msgMessage:
                    if (msgMessage.MsgHeader.PayloadType != 0)
                    {
                        return ThrowHelper.InvalidPayloadTypeException<InsertResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(InsertBodyReader, default).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;

                default:
                    return ThrowHelper.UnsupportedTypeException<InsertResult>(typeof(T));
            }
        }

        public static ValueTask WriteAsync(object message, ProtocolWriter protocol, CancellationToken token)
        {
            if (message is InsertMessage<T> insertMessage)
            {
                return protocol.WriteAsync(InsertMessageWriter, insertMessage, token); 
            }
            ThrowHelper.CursorException(message.GetType().ToString());
            return default;
        }
    }
}
