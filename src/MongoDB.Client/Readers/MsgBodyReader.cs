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
    internal class MsgBodyReader<T> : IMessageReader<Unit>
    {
        private readonly IGenericBsonSerializer<T> _serializer;
        private readonly MsgMessage _message;
        public readonly List<T> objects = new List<T>();
        private long _readed;
        private long _payloadLength;
        public bool Complete => _readed == _payloadLength;

        public MsgBodyReader(IGenericBsonSerializer<T> serializer, MsgMessage message)
        {
            _serializer = serializer;
            _message = message;
            _payloadLength = message.Header.MessageLength - 20; // message header + msg flags
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
        {
            var bsonReader = new MongoDBBsonReader(input);

            long consumedBytes = 0;
            while (_readed + consumedBytes < _payloadLength)
            {
                if (bsonReader.TryGetByte(out var payloadType))
                {
                    switch (payloadType)
                    {
                        case 0:
                            if (_serializer.GenericTryParse(ref bsonReader, out var item))
                            {
                                objects.Add(item);
                                consumedBytes = bsonReader.BytesConsumed;
                                consumed = bsonReader.Position;
                                examined = bsonReader.Position;
                            }
                            else
                            {
                                _readed += consumedBytes;
                                message = default;
                                return false;
                            }
                            break;
                        case 1:
                            // TODO: need to implement
                            return ThrowHelper.InvalidPayloadTypeException<bool>(payloadType);
                        default:
                           return ThrowHelper.InvalidPayloadTypeException<bool>(payloadType);
                    }
                }
                else
                {
                    _readed += consumedBytes;
                    message = default;
                    return false;
                }
            }

            _readed += consumedBytes;

            return Complete;
        }
    }
}
