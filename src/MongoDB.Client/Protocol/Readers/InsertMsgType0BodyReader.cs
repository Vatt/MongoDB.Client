using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Readers
{
    internal class InsertMsgType0BodyReader : IMessageReader<InsertResult>
    {
        private static readonly IGenericBsonSerializer<InsertResult> _resultSerializer;

        static InsertMsgType0BodyReader()
        {
            SerializersMap.TryGetSerializer(out _resultSerializer!);
            
        }
       
       public long Consumed { get; private set; }

        public bool TryParseMessage(
            in ReadOnlySequence<byte> input, 
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out InsertResult message)
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