using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class CreateCollectionBodyReader : IMessageReader<CreateCollectionResult>
    {
        public bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out CreateCollectionResult message)
        {
            var bsonReader = new BsonReader(input);


            if (CreateCollectionResult.TryParseBson(ref bsonReader, out var messageState, out var position) == false)
            {
                message = default;
                return false;
            }
            message = CreateCollectionResult.CreateMessage(messageState);
            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            return true;
        }
    }
}
