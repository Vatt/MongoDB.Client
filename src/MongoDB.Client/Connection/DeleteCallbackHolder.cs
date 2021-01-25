using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
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
        public static ValueTask WriteAsync(MongoRequestBase request, ProtocolWriter protocol, CancellationToken token)
        {
            var message = ((DeleteMongoRequest)request).Message;
            if (message != null)
            {
                return protocol.WriteAsync(ProtocolWriters.DeleteMessageWriter, message, token);
            }
            ThrowHelper.InsertException(request.GetType().ToString());
            return default;
        }
    }
}
