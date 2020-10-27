using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Readers
{
    internal class MsgType1BodyReader<T> : MsgBodyReader<T>
    {
        public MsgType1BodyReader(IGenericBsonSerializer<T> serializer, MsgMessage message)
            : base(serializer, message)
        {
        }

        public override bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
        {
            throw new NotImplementedException();
        }
    }
}
