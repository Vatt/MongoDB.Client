using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace MongoDB.Client.Readers
{
    internal class MsgBodyReader<T> : IMessageReader<T>
    {
        private readonly IGenericBsonSerializer<T> _serializer;
        private readonly MsgMessage message;
        private readonly List<object> objects = new List<object>();

        public MsgBodyReader(IGenericBsonSerializer<T> serializer, MsgMessage message)
        {
            _serializer = serializer;
            this.message = message;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out T message)
        {
            var bsonReader = new MongoDBBsonReader(input);
            if (_serializer.TryParse(ref bsonReader, out message))
            {
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                return true;
            }

            return false;
        }
    }
}
