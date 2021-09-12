using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Scheduler.Holders
{
    internal static partial class CursorCallbackHolder<T> where T : IBsonSerializer<T>
    {

        static unsafe CursorCallbackHolder()
        {

        }

        internal static async ValueTask<IParserResult> CursorParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            if (mongoResponse is ResponseMsgMessage msgMessage)
            {
                IMessageReader<CursorResult<T>> bodyReader;
                if (msgMessage.MsgHeader.PayloadType == 0)
                {
                    bodyReader = new FindMsgType0BodyReaderUnsafe<T>(msgMessage);
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
