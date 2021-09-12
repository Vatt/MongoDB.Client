using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Scheduler.Holders
{
    internal static class TransactionCallbackHolder
    {
        internal static async ValueTask<IParserResult> TransactionParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            if (mongoResponse is ResponseMsgMessage msgMessage)
            {
                if (msgMessage.MsgHeader.PayloadType == 0)
                {
                    var result = await reader.ReadAsync(ProtocolReaders.TransactionBodyReader).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                }

                return ThrowHelper.InvalidPayloadTypeException<TransactionResult>(msgMessage.MsgHeader.PayloadType);
            }

            return ThrowHelper.UnsupportedTypeException<TransactionResult>(typeof(TransactionResult));
        }
    }
}
