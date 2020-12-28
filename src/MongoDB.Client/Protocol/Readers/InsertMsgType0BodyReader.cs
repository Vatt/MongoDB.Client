using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class InsertMsgType0BodyReader : IMessageReader<InsertResult>
    {     
       public long Consumed { get; private set; }

        public bool TryParseMessage(
            in ReadOnlySequence<byte> input, 
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out InsertResult message)
        {
            var bsonReader = new BsonReader(input);


            if (InsertResult.TryParseBson(ref bsonReader, out message) == false)
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