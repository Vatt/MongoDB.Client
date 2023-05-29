using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class DropCollectionBodyReader : IMessageReader<DropCollectionResult>
    {
        public bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out DropCollectionResult message)
        {
            var bsonReader = new BsonReader(input);


            if (DropCollectionResult.TryParseBson(ref bsonReader, out var messageState, out var position) == false)
            {
                message = null;
                return false;
            }
            message = DropCollectionResult.CreateMessage(messageState);
            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            return true;
        }
    }
}
