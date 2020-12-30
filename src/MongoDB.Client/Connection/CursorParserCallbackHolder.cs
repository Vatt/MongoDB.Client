﻿using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Readers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal static partial class CursorParserCallbackHolder<T>
    {
        private static readonly Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> _parser;
        private static readonly IGenericBsonSerializer<T> _serializer;
        private static readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        internal static readonly unsafe delegate*<ref Bson.Reader.BsonReader, out T, bool> TryParseFnPtr;
        static CursorParserCallbackHolder()
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
                    if (_serializer == null)
                    {
                        bodyReader = new FindMsgType0BodyReaderUnsafe<T>(msgMessage);
                    }
                    else
                    {
                        bodyReader = new FindMsgType0BodyReader<T>(_serializer, msgMessage);
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