using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Readers
{
    internal abstract class MsgBodyReader<T> : IMessageReader<Unit>
    {
        public CursorResult<T> CursorResult { get; private set; }
        protected readonly IGenericBsonSerializer<T> Serializer;
        protected readonly ResponseMsgMessage Message;
        public bool Complete { get; protected set; }

        public MsgBodyReader(IGenericBsonSerializer<T> serializer, ResponseMsgMessage message)
        {
            Serializer = serializer;
            Message = message;
            CursorResult = new CursorResult<T>
            {
                Cursor = new Cursor<T>
                {
                    Items = new List<T>()
                }
            };
        }

        public abstract bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message);
    }
}
