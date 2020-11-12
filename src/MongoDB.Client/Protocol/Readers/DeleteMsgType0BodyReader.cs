using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Readers
{
    internal class DeleteMsgType0BodyReader : IMessageReader<BsonDocument>
    {
        private static readonly IGenericBsonSerializer<BsonDocument> _resultSerializer;

        static DeleteMsgType0BodyReader()
        {
            SerializersMap.TryGetSerializer(out _resultSerializer!);
        }
       
       public long Consumed { get; private set; }

        public bool TryParseMessage(
            in ReadOnlySequence<byte> input, 
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out BsonDocument message)
        {
            var bsonReader = new BsonReader(input);


            if (_resultSerializer.TryParse(ref bsonReader, out message) == false)
            {
                return false;
            }
            
            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            Consumed = bsonReader.BytesConsumed;

            return true;
        }
    }
}