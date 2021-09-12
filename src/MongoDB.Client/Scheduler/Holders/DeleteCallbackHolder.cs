using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Scheduler.Holders
{
    internal static class DeleteCallbackHolder
    {
        internal static async ValueTask<IParserResult> DeleteParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            switch (mongoResponse)
            {
                case ResponseMsgMessage msgMessage:
                    if (msgMessage.MsgHeader.PayloadType != 0)
                    {
                        return ThrowHelper.InvalidPayloadTypeException<DeleteResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(ProtocolReaders.DeleteMsgType0BodyReader).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                default:
                    return ThrowHelper.UnsupportedTypeException<DeleteResult>(typeof(DeleteResult));
            }
        }
    }
}
