using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;
using System;
using System.Threading;
using System.Threading.Tasks;

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

        public static ValueTask WriteAsync(IMongoInsertMessage message, ProtocolWriter protocol, CancellationToken token) 
        {
            var insertMessage = message as InsertMessage<T>;
            if (message != null)
            {
                return protocol.WriteAsync(InsertMessageWriter, insertMessage, token);
            }
            ThrowHelper.InsertException(message.GetType().ToString());
            return default;
        }
    }
}
