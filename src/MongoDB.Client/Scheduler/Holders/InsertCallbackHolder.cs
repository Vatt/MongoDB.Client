using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;

namespace MongoDB.Client.Scheduler.Holders
{
    internal static class InsertCallbackHolder<T>
    {
        private static unsafe readonly delegate*<ref BsonWriter, in T, void> WriterFnPtr;
        public static Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>>? Parser;
        private static readonly IMessageWriter<InsertMessage<T>> InsertMessageWriter;
        private static readonly InsertMsgType0BodyReader InsertBodyReader = new InsertMsgType0BodyReader();

        static unsafe InsertCallbackHolder()
        {
            WriterFnPtr = SerializerFnPtrProvider<T>.WriteFnPtr;
            if (WriterFnPtr != null)
            {
                InsertMessageWriter = new InsertMessageWriterUnsafe<T>();
            }
            else if (SerializersMap.TryGetSerializer<T>(out var serializer))
            {
                InsertMessageWriter = new InsertMessageWriter<T>(serializer);
            }
            else
            {
                throw new MongoException($"Serializer for type '{typeof(T)}' does not found");
            }
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

        public static ValueTask WriteAsync(InsertMessage<T> message, ProtocolWriter protocol, CancellationToken token)
        {
            return protocol.WriteAsync(InsertMessageWriter, message, token);
        }
    }
}
