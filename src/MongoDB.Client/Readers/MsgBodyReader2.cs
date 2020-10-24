//using MongoDB.Client.Bson.Reader;
//using MongoDB.Client.Bson.Serialization;
//using MongoDB.Client.Messages;
//using MongoDB.Client.Protocol.Core;
//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;

//namespace MongoDB.Client.Readers
//{
//    internal class MsgBodyReader2<T> : IMessageReader<Unit>
//    {
//        private readonly IGenericBsonSerializer<T> _serializer;
//        private readonly MsgMessage _message;
//        public List<T>? Objects { get; }
//        public T? Item { get; private set; }
//        public Cursor Cursor { get; private set; }
//        private long _readed;
//        private long _payloadLength;
//        private ParserState _state;
//        public bool Complete => _readed == _payloadLength;

//        public MsgBodyReader2(IGenericBsonSerializer<T> serializer, MsgMessage message, bool oneItem)
//        {
//            _serializer = serializer;
//            _message = message;
//            _payloadLength = message.Header.MessageLength - 20; // message header + msg flags
//            _state = ParserState.Cursor;
//            if (oneItem == false)
//            {
//                Objects = new List<T>();
//            }
//        }

//        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
//        {
//            var bsonReader = new MongoDBBsonReader(input);

//            long consumedBytes = 0;


//            while (_readed + consumedBytes < _payloadLength)
//            {
//                switch (_state)
//                {
//                    case ParserState.Cursor:
//                        if (bsonReader.TryGetByte(out var payloadType))
//                        {
//                            switch (payloadType)
//                            {
//                                case 0:
//                                    if (_serializer.TryParse(ref bsonReader, out var item))
//                                    {
//                                        objects.Add(item);
//                                        consumedBytes = bsonReader.BytesConsumed;
//                                        consumed = bsonReader.Position;
//                                        examined = bsonReader.Position;
//                                    }
//                                    else
//                                    {
//                                        _readed += consumedBytes;
//                                        message = default;
//                                        return false;
//                                    }
//                                    break;
//                                case 1:
//                                    // TODO: need to implement
//                                    return ThrowHelper.InvalidPayloadTypeException<bool>(payloadType);
//                                default:
//                                    return ThrowHelper.InvalidPayloadTypeException<bool>(payloadType);
//                            }
//                        }
//                        else
//                        {
//                            _readed += consumedBytes;
//                            message = default;
//                            return false;
//                        }
//                        break;
//                    case ParserState.Models:
//                        break;
//                    case ParserState.Complete:
//                    default:
//                        return true;
//                }


//            }

//            _readed += consumedBytes;

//            return Complete;
//        }


//        private enum ParserState
//        {
//            Cursor, Models, Complete
//        }
//    }
//}
