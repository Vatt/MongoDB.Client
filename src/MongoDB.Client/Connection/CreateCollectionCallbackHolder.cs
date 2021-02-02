using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System.Threading.Tasks;
using MongoDB.Client.Protocol;
using System.Threading;

namespace MongoDB.Client.Connection
{
    internal static class CreateCollectionCallbackHolder
    {
        public static RequestCompletion CreateCompletion(ManualResetValueTaskSource<IParserResult> src) => new RequestCompletion(src, CreateCollectionParseAsync);
        internal static async ValueTask<IParserResult> CreateCollectionParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            switch (mongoResponse)
            {
                case ResponseMsgMessage msgMessage:
                    if (msgMessage.MsgHeader.PayloadType != 0)
                    {
                        return ThrowHelper.InvalidPayloadTypeException<CreateCollectionResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(ProtocolReaders.CreateCollectionBodyReader).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                default:
                    return ThrowHelper.UnsupportedTypeException<CreateCollectionResult>(typeof(CreateCollectionResult));
            }
        }
    }
}
