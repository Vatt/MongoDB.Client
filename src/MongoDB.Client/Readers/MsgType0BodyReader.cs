using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Readers
{
    internal class MsgType0BodyReader<T> : MsgBodyReader<T>
    {
        public MsgType0BodyReader(IGenericBsonSerializer<T> serializer, MsgMessage message)
            :base(serializer, message)
        {
        }

        public override bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
        {
            var bsonReader = new MongoDBBsonReader(input);

            if (Serializer.TryParse(ref bsonReader, out var item))
            {
                Objects.Add(item);
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                Complete = true;
                return true;
            }
            else
            {
                message = default;
                return false;
            }
        }
    }
}
