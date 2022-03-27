using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class UpdateMsgType0BodyReader : IMessageReader<UpdateResult>
    {
        public bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out UpdateResult message)
        {
            var bsonReader = new BsonReader(input);
//#if DEBUG
//            bsonReader.TryParseDocument(out var doc);
//            bsonReader = new BsonReader(input);
//#endif
            if (UpdateResult.TryParseBson(ref bsonReader, out message) == false)
            {
                return false;
            }

            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            return true;
        }
    }
}
