using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Scheduler.Holders
{
    internal static class DropCollectionCallbackHolder
    {
        internal static async ValueTask<IParserResult> DropCollectionParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            switch (mongoResponse)
            {
                case ResponseMsgMessage msgMessage:
                    if (msgMessage.MsgHeader.PayloadType != 0)
                    {
                        return ThrowHelper.InvalidPayloadTypeException<DropCollectionResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(ProtocolReaders.DropCollectionBodyReader).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                default:
                    return ThrowHelper.UnsupportedTypeException<DropCollectionResult>(typeof(DropCollectionResult));
            }
        }
    }
}
