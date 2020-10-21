using System;
using System.Buffers;
using AMQP.Client.RabbitMQ.Protocol.Core;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client
{
    public class BodyReader : IMessageReader<object>
    {
        private readonly IBsonSerializable _serializer;

        public BodyReader(IBsonSerializable serializer)
        {
            _serializer = serializer;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out object message)
        {
            var bsonReader = new MongoDBBsonReader(input);
            if (_serializer.TryParse(ref bsonReader, ref consumed, ref examined, out message))
            {
                return true;
            }

            return false;
        }
    }
}
