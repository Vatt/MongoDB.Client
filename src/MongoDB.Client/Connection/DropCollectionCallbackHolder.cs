using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using System.Threading.Tasks;
using MongoDB.Client.Protocol.Readers;

namespace MongoDB.Client.Connection
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
                        return ThrowHelper.InvalidPayloadTypeException<BsonParseResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(new BsonBodyReader()).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                default:
                    return ThrowHelper.UnsupportedTypeException<BsonParseResult>(typeof(BsonParseResult));
            }
        }
    }
}
