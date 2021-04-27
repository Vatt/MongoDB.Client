using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Protocol.Readers
{
    internal class FindMsgType0BodyReaderUnsafe<T> : MsgBodyReader<T>
    {

        public FindMsgType0BodyReaderUnsafe(ResponseMsgMessage message)
            : base(null!, message)
        {

        }


        public override bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out CursorResult<T> message)
        {
            message = _cursorResult;
            var bsonReader = new BsonReader(input);
            var a = 1;
            if (a == 2)
            {
                bsonReader.TryParseDocument(out var doc);
            }
            if (CursorResult<T>.TryParseBson(ref bsonReader, out message))
            {
                message.MongoCursor.Items ??= new();
                if (message.MongoCursor.FirstBatch is not null)
                {
                    message.MongoCursor.Items.AddRange(message.MongoCursor.FirstBatch);
                }
                if (message.MongoCursor.NextBatch is not null)
                {
                    message.MongoCursor.Items.AddRange(message.MongoCursor.NextBatch);
                }
                return true;
            }
            return false;
   
        }
    }
}
