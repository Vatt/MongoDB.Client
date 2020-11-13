using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Utils;

namespace MongoDB.Client.Protocol.Readers
{
    internal abstract class MsgBodyReader<T> : IMessageReader<Unit>
    {
        public CursorResult<T> CursorResult { get; private set; }
        protected readonly IGenericBsonSerializer<T> Serializer;
        protected readonly ResponseMsgMessage Message;
        public bool Complete { get; protected set; }

        
        protected long _readed;
        public long Readed => _readed;
        
        public MsgBodyReader(IGenericBsonSerializer<T> serializer, ResponseMsgMessage message)
        {
            Serializer = serializer;
            Message = message;
            CursorResult = new CursorResult<T>
            {
                MongoCursor = new MongoCursor<T>
                {
                    Items = ListsPool<T>.Pool.Get()
                }
            };
        }

        public abstract bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message);
    }
}
