using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Readers
{
    internal class ReplyBodyReader<T> : IMessageReader<T>
    {
        private readonly IGenericBsonSerializer<T> _serializer;

        public ReplyBodyReader(IGenericBsonSerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out T message)
        {
            var bsonReader = new MongoDBBsonReader(input);
            if (_serializer.GenericTryParse(ref bsonReader, out message))
            {
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                return true;
            }
            message = default;
            return false;
        }
    }
}
