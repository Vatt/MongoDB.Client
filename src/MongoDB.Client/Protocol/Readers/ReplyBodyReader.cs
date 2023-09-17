using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class ReplyBodyReader<T> : IMessageReader<QueryResult<T>> where T : IBsonSerializer<T>
    {
        private readonly ReplyMessage _replyMessage;
        private readonly QueryResult<T> _result;

        public ReplyBodyReader(ReplyMessage replyMessage)
        {
            _replyMessage = replyMessage;
            _result = new QueryResult<T>(_replyMessage.ReplyHeader.CursorId);
        }

        public unsafe bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out QueryResult<T> message)
        {
            message = _result;
            var bsonReader = new BsonReader(input);
            while (_result.Count < _replyMessage.ReplyHeader.NumberReturned)
            {
                if (T.TryParseBson(ref bsonReader, out var item))
                {
                    _result.Add(item);
                    consumed = bsonReader.Position;
                    examined = bsonReader.Position;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }
}
