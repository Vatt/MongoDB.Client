using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class BsonBodyReader : IMessageReader<BsonParseResult>
    {
        public long Consumed { get; private set; }

        public bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out BsonParseResult message)
        {
            var bsonReader = new BsonReader(input);


            if (BsonDocument.TryParseBson(ref bsonReader, out var document) == false)
            {
                examined = input.End;
                message = default;
                return false;
            }

            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            Consumed = bsonReader.BytesConsumed;
            message = new BsonParseResult(document);
            return true;
        }
    }
}
