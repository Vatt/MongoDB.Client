﻿using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class MongoPingMesageReader : IMessageReader<MongoPingMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out MongoPingMessage message)
        {
            var bsonReader = new BsonReader(input);
            if (MongoPingMessage.TryParseBson(ref bsonReader, out message))
            {
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                return true;
            }
            else
            {
                examined = input.End;
                message = default;
                return false;
            }
        }
    }
}
