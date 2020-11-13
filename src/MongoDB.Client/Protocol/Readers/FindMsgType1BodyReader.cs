﻿using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Protocol.Readers
{
    internal class FindMsgType1BodyReader<T> : MsgBodyReader<T>
    {
        public FindMsgType1BodyReader(IGenericBsonSerializer<T> serializer, ResponseMsgMessage message)
            : base(serializer, message)
        {
        }

        public override bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
        {
            throw new NotImplementedException();
        }
    }
}
