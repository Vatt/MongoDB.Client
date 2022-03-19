using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;

namespace MongoDB.Client.Scheduler.Holders
{
    internal static class InsertCallbackHolder<T> //where T : IBsonSerializer<T>
    {
        private static readonly IMessageWriter<InsertMessage<T>> InsertMessageWriter;
        private static readonly InsertMsgType0BodyReader InsertBodyReader = new InsertMsgType0BodyReader();

        static unsafe InsertCallbackHolder()
        {
            InsertMessageWriter = new InsertMessageWriterUnsafe<T>();
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
