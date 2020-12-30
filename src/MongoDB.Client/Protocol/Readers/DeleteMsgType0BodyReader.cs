using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Protocol.Readers
{
    internal class DeleteMsgType0BodyReader : IMessageReader<DeleteResult>
    {
        public bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out DeleteResult message)
        {
            var bsonReader = new BsonReader(input);


            if (DeleteResult.TryParseBson(ref bsonReader, out message) == false)
            {
                return false;
            }

            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            return true;
        }
    }
}