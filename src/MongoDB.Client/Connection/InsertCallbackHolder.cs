using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using MongoDB.Client.Protocol.Writers;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal static class InsertCallbackHolder<T>
    {
        private static readonly InsertMsgType0BodyReader InsertBodyReader = new InsertMsgType0BodyReader();
        private static unsafe readonly delegate*<ref BsonWriter, in T, void> WriterFnPtr;
        public static readonly IMessageWriter<InsertMessage<T>> InsertMessageWriter;
        public static RequestCompletion CreateCompletion(ManualResetValueTaskSource<IParserResult> src) => new RequestCompletion(src, InsertParseAsync); 
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
    }
}
