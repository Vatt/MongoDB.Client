using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Readers
{
    internal class ReplyBodyReader<T> : IMessageReader<T>
    {
        private readonly IGenericBsonSerializer<T> _serializer;
        private readonly ReplyMessage _replyMessage;
        private readonly QueryResult<T> _result;

        public QueryResult<T> Result => _result;

        public ReplyBodyReader(IGenericBsonSerializer<T> serializer, ReplyMessage replyMessage)
        {
            _serializer = serializer;
            _replyMessage = replyMessage;
            _result = new QueryResult<T>(_replyMessage.ReplyHeader.CursorId);
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out T message)
        {
            var bsonReader = new BsonReader(input);
            for (int i = 0; i < _replyMessage.ReplyHeader.NumberReturned; i++)
            {
                if (_serializer.TryParse(ref bsonReader, out message))
                {
                    _result.Add(message);
                    consumed = bsonReader.Position;
                    examined = bsonReader.Position;
                }
                else
                {
                    return false;
                }
            }

            message = default!;
            return true;
        }
    }
}