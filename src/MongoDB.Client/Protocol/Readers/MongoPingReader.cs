using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Protocol.Readers
{
    internal class MongoPingMesageReader : IMessageReader<MongoPingMessage>
    {
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out MongoPingMessage message)
        {
            message = default;
            var bsonReader = new BsonReader(input);
            if (MongoPingMessage.TryParseBson(ref bsonReader, out message))
            {
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
