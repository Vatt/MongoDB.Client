using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using System;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal static partial class CursorCallbackHolder<T>
    {
        private static readonly IGenericBsonSerializer<T> _serializer;
        internal static readonly unsafe delegate*<ref Bson.Reader.BsonReader, out T, bool> TryParseFnPtr;
        static CursorCallbackHolder()
        {
            SerializersMap.TryGetSerializer(out _serializer);
            unsafe
            {
                TryParseFnPtr = SerializerFnPtrProvider<T>.TryParseFnPtr;
            }
        }

        internal static async ValueTask<IParserResult> CursorParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            if (mongoResponse is ResponseMsgMessage msgMessage)
            {
                IMessageReader<CursorResult<T>> bodyReader;
                if (msgMessage.MsgHeader.PayloadType == 0)
                {
                    unsafe
                    {
                        if (TryParseFnPtr != default)
                        {
                            bodyReader = new FindMsgType0BodyReaderUnsafe<T>(msgMessage);
                        }
                        else
                        {
                            bodyReader = new FindMsgType0BodyReader<T>(_serializer, msgMessage);
                        }
                    }

                    var result = await reader.ReadAsync(bodyReader).ConfigureAwait(false);
                    reader.Advance();
                    return result.Message;
                }
                return ThrowHelper.InvalidPayloadTypeException<CursorResult<T>>(msgMessage.MsgHeader.PayloadType);
            }
            return ThrowHelper.UnsupportedTypeException<CursorResult<T>>(typeof(T));
        }
    }
}