using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace MongoDB.Client.Readers
{
    internal class MsgBodyReader : IMessageReader<object>
    {
        private readonly IBsonSerializable _serializer;
        private readonly MsgMessage message;
        private readonly List<object> objects = new List<object>();

        public MsgBodyReader(IBsonSerializable serializer, MsgMessage message)
        {
            _serializer = serializer;
            this.message = message;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out object message)
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
