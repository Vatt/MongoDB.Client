using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Protocol.Readers
{
    internal class FindMsgType0BodyReaderUnsafe<T> : MsgBodyReader<T>
        where T : IBsonSerializer<T>
    {
        private long _payloadLength;
        private CursorResult<T>.CursorResultState state;

        public FindMsgType0BodyReaderUnsafe(ResponseMsgMessage message)
            : base(message)
        {
            _payloadLength = message.Header.MessageLength;
            state = new CursorResult<T>.CursorResultState();
        }


        public unsafe override bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out CursorResult<T> message)
        {
            message = default;

            var bsonReader = new BsonReader(input);

            var isComplete = CursorResult<T>.TryParseBson(ref bsonReader, state);

            consumed = state.Position;
            examined = bsonReader.End;

            if (isComplete is false)
            {
                return false;
            }

            message = state.CreateMessage();

            return true;
        }
    }
}
