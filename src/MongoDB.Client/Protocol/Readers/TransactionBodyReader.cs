using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class TransactionBodyReader : IMessageReader<TransactionResult>
    {
        public bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out TransactionResult message)
        {
            var bsonReader = new BsonReader(input);


            if (TransactionResult.TryParseBson(ref bsonReader, out var state, out var position) == false)
            {
                message = default;
                return false;
            }

            message = TransactionResult.CreateMessage(state);
            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            return true;
        }
    }
}
