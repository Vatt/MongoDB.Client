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
        protected readonly IGenericBsonSerializer<T> Serializer;
        protected readonly MsgMessage Message;
        public readonly List<T> objects = new List<T>();
        public bool Complete { get; protected set; }

        public MsgBodyReader(IGenericBsonSerializer<T> serializer, MsgMessage message)
        {
            Serializer = serializer;
            Message = message;
        }

        public abstract bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message);
    }
}
