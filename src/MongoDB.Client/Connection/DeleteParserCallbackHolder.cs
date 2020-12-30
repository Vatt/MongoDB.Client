﻿using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal static class DeleteParserCallbackHolder
    {
        internal static async ValueTask<IParserResult> DeleteParseAsync(ProtocolReader reader, MongoResponseMessage mongoResponse)
        {
            switch (mongoResponse)
            {
                case ResponseMsgMessage msgMessage:

                    DeleteMsgType0BodyReader bodyReader;
                    if (msgMessage.MsgHeader.PayloadType == 0)
                    {
                        bodyReader = new DeleteMsgType0BodyReader();
                    }
                    else
                    {
                        return ThrowHelper.InvalidPayloadTypeException<DeleteResult>(msgMessage.MsgHeader.PayloadType);
                    }

                    var result = await reader.ReadAsync(bodyReader).ConfigureAwait(false);
                    reader.Advance();

                    return result.Message;
                default:
                    return ThrowHelper.UnsupportedTypeException<DeleteResult>(typeof(DeleteResult));
            }
        }
    }
}
