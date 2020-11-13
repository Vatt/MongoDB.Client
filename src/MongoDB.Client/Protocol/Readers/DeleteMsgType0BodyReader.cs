﻿using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Protocol.Readers
{
    internal class DeleteMsgType0BodyReader : IMessageReader<DeleteResult>
    {
        private static readonly IGenericBsonSerializer<DeleteResult> _resultSerializer;

        static DeleteMsgType0BodyReader()
        {
            SerializersMap.TryGetSerializer(out _resultSerializer!);
        }
       
       public long Consumed { get; private set; }

        public bool TryParseMessage(
            in ReadOnlySequence<byte> input, 
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out DeleteResult message)
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