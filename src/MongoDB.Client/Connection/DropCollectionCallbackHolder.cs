using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System.Threading.Tasks;
using MongoDB.Client.Protocol;
using System.Threading;

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
                        return ThrowHelper.InvalidPayloadTypeException<DropCollectionResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(ProtocolReaders.DropCollectionBodyReader).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                default:
                    return ThrowHelper.UnsupportedTypeException<DropCollectionResult>(typeof(DropCollectionResult));
            }
        }
        public static ValueTask WriteAsync(MongoRequestBase request, ProtocolWriter protocol, CancellationToken token)
        {
            var message = ((DropCollectionMongoRequest)request).Message;
            if (message != null)
            {
                return protocol.WriteAsync(ProtocolWriters.DropCollectionMessageWriter, message, token);
            }
            ThrowHelper.InsertException(request.GetType().ToString());
            return default;
        }
    }
}
