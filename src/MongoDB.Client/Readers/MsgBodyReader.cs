using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Readers
{
    internal class MsgBodyReader<T> : IMessageReader<T>
    {
        private readonly IGenericBsonSerializer<T> _serializer;
        private readonly MsgMessage _message;
        public readonly List<T> objects = new List<T>();
        private long _remained;
        public bool Complete => _remained == 0;

        public MsgBodyReader(IGenericBsonSerializer<T> serializer, MsgMessage message)
        {
            _serializer = serializer;
            _message = message;
            _remained = message.Header.MessageLength;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out T message)
        {
            var bsonReader = new MongoDBBsonReader(input);
            long consumedBytes = 0;
            while (_serializer.GenericTryParse(ref bsonReader, out message))
            {
                objects.Add(message);
                consumedBytes = bsonReader.BytesConsumed;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
            }

            _remained -= consumedBytes;


            return Complete;
        }
    }
}
