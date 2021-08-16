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
    internal class FindMsgType0BodyReaderUnsafe<T> : MsgBodyReader<T> where T : IBsonSerializer<T>
    {
        private long _modelsReaded;
        private long _payloadLength;
        private long _modelsLength;
        private long _docLength;
        private long _docReaded;

        private ParserState _state;


        public FindMsgType0BodyReaderUnsafe(ResponseMsgMessage message)
            : base(message)
        {
            _payloadLength = message.Header.MessageLength;
            _state = ParserState.Initial;
        }


        public override bool TryParseMessage(
            in ReadOnlySequence<byte> input,
            ref SequencePosition consumed,
            ref SequencePosition examined,
            [MaybeNullWhen(false)] out CursorResult<T> message)
        {
            message = _cursorResult;
            var bsonReader = new BsonReader(input);

            if (_state == ParserState.Initial)
            {
                var checkpoint = bsonReader.BytesConsumed;
                if (TryReadCursorStart(ref bsonReader, out var modelsLength, out var docLength, out var hasBatch) ==
                    false)
                {
                    return false;
                }

                _modelsLength = modelsLength - 4;
                _docLength = docLength;
                _docReaded = bsonReader.BytesConsumed - checkpoint;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                Advance(_docReaded);
                _state = hasBatch ? ParserState.Models : ParserState.Complete;
            }

            if (_state == ParserState.Models)
            {
                var items = message.MongoCursor.Items;
                while (_modelsReaded < _modelsLength - 1)
                {
                    var checkpoint = bsonReader.BytesConsumed;
#if DEBUG
                    if (bsonReader.TryGetByte(out var type) == false)
                    {
                        return false;
                    }

                    if (bsonReader.TryGetCString(out var name) == false)
                    {
                        return false;
                    }
#else
                    if (bsonReader.TryAdvanceTo(0) == false)
                    {
                        return false;
                    }
#endif
                    if (bsonReader.TryPeekInt32(out int modelLength) && bsonReader.Remaining >= modelLength)
                    {
                        bool tryParseResult = default;
                        T? item = default;
                        unsafe
                        {
                            //TODO: FIX IT
                            //tryParseResult = SerializerFnPtrProvider<T>.TryParseFnPtr(ref bsonReader, out item);
                            tryParseResult = T.TryParseBson(ref bsonReader, out item);
                        }
                        if (tryParseResult)
                        {
                            items.Add(item);
                            _modelsReaded += bsonReader.BytesConsumed - checkpoint;
                            Advance(bsonReader.BytesConsumed - checkpoint);
                            consumed = bsonReader.Position;
                            examined = bsonReader.Position;
                        }
                        else
                        {
                            ThrowHelper.InvalidBsonException();
                        }
                    }
                    else
                    {
                        message = default;
                        return false;
                    }
                }

                _state = ParserState.ModelsEnd;
            }

            if (_state == ParserState.ModelsEnd)
            {
                if (bsonReader.TryGetByte(out var endDocumentMarker) == false)
                {
                    return false;
                }
                _docReaded += _modelsLength + 1;
                Advance(1);
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Cursor;
            }

            if (_state == ParserState.Cursor)
            {
                var checkpoint = bsonReader.BytesConsumed;
                if (TryReadCursorEnd(ref bsonReader, checkpoint) == false)
                {
                    return false;
                }

                Advance(bsonReader.BytesConsumed - checkpoint);
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Complete;
            }


            Debug.Assert(Message.Consumed == Message.Header.MessageLength);

            Complete = _state == ParserState.Complete;

            return true;
        }


        private bool TryReadCursorStart(ref BsonReader reader, out int modelsLength, out int docLength, out bool hasBatch)
        {
#if DEBUG
            string? name;
#else
            ReadOnlySpan<byte> name;
#endif

            modelsLength = 0;
            hasBatch = false;

            byte endMarker;
            bool hasItems;
            var checkpoint = reader.BytesConsumed;
            if (!reader.TryGetInt32(out docLength)) { return false; }
            do
            {

                if (TryGetName(ref reader, out name) == false) { return false; }
                if (NameEquals(name, CursorSpan))
                {
                    var initConsumed = reader.BytesConsumed;
                    if (!reader.TryGetInt32(out int cursorLength)) { return false; }

                    while (reader.BytesConsumed - initConsumed < cursorLength - 1)
                    {
                        if (TryGetName(ref reader, out name) == false) { return false; }
                        if (TryParseValue(ref reader, name, out hasItems, out modelsLength) == false) { return false; }
                        if (hasItems)
                        {
                            hasBatch = true;
                            return true;
                        }
                    }


                    if (!reader.TryPeekByte(out endMarker))
                    {
                        return false;
                    }

                    if (endMarker is 0)
                    {
                        reader.TryGetByte(out _);

                        if (TryGetName(ref reader, out name) == false)
                        {
                            return false;
                        }

                        if (TryParseValue(ref reader, name, out _, out modelsLength) == false)
                        {
                            return false;
                        }

                        if (!reader.TryGetByte(out endMarker))
                        {
                            return false;
                        }

                        if (endMarker is 0)
                        {
                            return true;
                        }
                    }

                    if (TryGetClusterInfo(ref reader, out _, out _) == false)
                    {
                        return false;
                    }

                    if (!reader.TryGetByte(out endMarker))
                    {
                        return false;
                    }
                    if (endMarker is 0)
                    {
                        return true;
                    }
                    return ThrowHelper.MissedDocumentEndMarkerException<bool>();
                }


                if (NameEquals(name, OkSpan))
                {
                    if (!reader.TryGetDouble(out double okValue)) { return false; }
                    _cursorResult.Ok = okValue;
                    continue;
                }

                if (NameEquals(name, ErrorMessageSpan))
                {
                    if (!reader.TryGetString(out var errorValue)) { return false; }
                    _cursorResult.ErrorMessage = errorValue;
                    continue;
                }

                if (NameEquals(name, CodeSpan))
                {
                    if (!reader.TryGetInt32(out int codeValue)) { return false; }
                    _cursorResult.Code = codeValue;
                    continue;
                }

                if (NameEquals(name, CodeNameSpan))
                {
                    if (!reader.TryGetString(out var codeNameValue)) { return false; }
                    _cursorResult.CodeName = codeNameValue;
                    continue;
                }
            }
            while (reader.BytesConsumed - checkpoint < docLength - 1);

            if (!reader.TryGetByte(out endMarker)) { return false; }
            if (endMarker is 0)
            {
                return true;
            }
            return ThrowHelper.MissedDocumentEndMarkerException<bool>();
        }

        private bool TryReadCursorEnd(ref BsonReader reader, long checkpoint)
        {
#if DEBUG
            string? name;
#else
            ReadOnlySpan<byte> name;
#endif
            if (TryGetName(ref reader, out name) == false) { return false; }
            if (TryParseValue(ref reader, name, out _, out _) == false) { return false; }

            if (TryGetName(ref reader, out name) == false) { return false; }
            if (TryParseValue(ref reader, name, out _, out _) == false) { return false; }

            byte endMarker;
            if (!reader.TryGetByte(out endMarker)) { return false; }
            if (endMarker is 0)
            {
                if (TryGetName(ref reader, out name) == false) { return false; }
                if (TryParseValue(ref reader, name, out _, out _) == false) { return false; }
                var readed = reader.BytesConsumed - checkpoint;
                var docReaded = _docReaded + readed;
                if (_docLength > docReaded)
                {
                    if (TryGetClusterInfo(ref reader, out _, out _) == false)
                    {
                        return false;
                    }
                }
                if (!reader.TryGetByte(out endMarker)) { return false; }
                if (endMarker is 0)
                {
                    return true;
                }
            }

            return ThrowHelper.MissedDocumentEndMarkerException<bool>();
        }

        private static bool TryGetClusterInfo(ref BsonReader reader, [MaybeNullWhen(false)] out MongoClusterTime clusterTime, out BsonTimestamp timestamp)
        {
#if DEBUG
            string? name;
#else
            ReadOnlySpan<byte> name;
#endif
            if (TryGetName(ref reader, out name) == false)
            {
                clusterTime = default;
                timestamp = default;
                return false;
            }
            if (MongoClusterTime.TryParseBson(ref reader, out clusterTime) == false)
            {
                clusterTime = default;
                timestamp = default;
                return false;
            }
            if (TryGetName(ref reader, out name) == false)
            {
                clusterTime = default;
                timestamp = default;
                return false;
            }
            if (reader.TryGetTimestamp(out timestamp) == false)
            {
                return false;
            }

            return true;
        }

#if DEBUG
        private bool TryParseValue(ref BsonReader reader, string name, out bool hasItems, out int modelsLength)
#else
        private bool TryParseValue(ref BsonReader reader, ReadOnlySpan<byte> name, out bool hasItems, out int modelsLength)
#endif
        {
            hasItems = false;
            modelsLength = 0;
            if (NameEquals(name, IdSpan))
            {
                if (!reader.TryGetInt64(out long idValue)) { return false; }
                _cursorResult.MongoCursor.Id = idValue;
                return true;
            }

            if (NameEquals(name, NsSpan))
            {
                if (!reader.TryGetString(out var nsValue)) { return false; }
                _cursorResult.MongoCursor.Namespace = nsValue;
                return true;
            }

            if (NameEquals(name, OkSpan))
            {
                if (!reader.TryGetDouble(out double okValue)) { return false; }
                _cursorResult.Ok = okValue;
                return true;
            }

            if (NameEquals(name, FirstBatchSpan) || NameEquals(name, NextBatchSpan))
            {
                if (!reader.TryGetInt32(out modelsLength)) { return false; }
                if (modelsLength == 5)
                {
                    if (!reader.TryGetByte(out var endMarker)) { return false; }
                    return true;
                }
                else
                {
                    hasItems = true;
                    return true;
                }
            }

#if DEBUG
            return ThrowHelper.UnknownCursorFieldException<bool>(name);
#else
            return ThrowHelper.UnknownCursorFieldException<bool>(System.Text.Encoding.UTF8.GetString(name));
#endif
        }


#if DEBUG
        private static bool NameEquals(string name, ReadOnlySpan<byte> other)
        {
            var otherName = System.Text.Encoding.UTF8.GetString(other);
            return name.Equals(otherName, StringComparison.Ordinal);
        }

        private static bool TryGetName(ref BsonReader reader, [MaybeNullWhen(false)] out string name)
        {
            if (!reader.TryGetByte(out _)) { name = default; return false; }
            if (!reader.TryGetCString(out name)) { return false; }
            return true;
        }
#else
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool NameEquals(ReadOnlySpan<byte> name, ReadOnlySpan<byte> other)
        {
            return name.SequenceEqual(other);
        }

        private static bool TryGetName(ref BsonReader breader, out ReadOnlySpan<byte> name)
        {
            if (!breader.TryGetByte(out _)) { name = default; return false; }
            if (!breader.TryGetCStringAsSpan(out name)) { return false; }
            return true;
        }
#endif


        private static ReadOnlySpan<byte> CursorSpan => new byte[] { 99, 117, 114, 115, 111, 114 }; // cursor

        private static ReadOnlySpan<byte> FirstBatchSpan =>
            new byte[] { 102, 105, 114, 115, 116, 66, 97, 116, 99, 104 }; // firstBatch

        private static ReadOnlySpan<byte> NextBatchSpan =>
            new byte[] { 110, 101, 120, 116, 66, 97, 116, 99, 104 }; // nextBatch

        private static ReadOnlySpan<byte> IdSpan => new byte[] { 105, 100 }; // id
        private static ReadOnlySpan<byte> NsSpan => new byte[] { 110, 115 }; // ns
        private static ReadOnlySpan<byte> OkSpan => new byte[] { 111, 107 }; // ok
        private static ReadOnlySpan<byte> ErrorMessageSpan => new byte[] { 101, 114, 114, 109, 115, 103 }; // errmsg
        private static ReadOnlySpan<byte> CodeSpan => new byte[] { 99, 111, 100, 101 }; // code
        private static ReadOnlySpan<byte> CodeNameSpan => new byte[] { 99, 111, 100, 101, 78, 97, 109, 101 }; // codeName

        private enum ParserState
        {
            Initial,
            Models,
            ModelsEnd,
            Cursor,
            Complete
        }
    }
}
//using System;
//using System.Buffers;
//using System.Diagnostics;
//using System.Diagnostics.CodeAnalysis;
//using MongoDB.Client.Bson.Document;
//using MongoDB.Client.Bson.Reader;
//using MongoDB.Client.Bson.Serialization;
//using MongoDB.Client.Exceptions;
//using MongoDB.Client.Messages;

//namespace MongoDB.Client.Protocol.Readers
//{
//    internal class FindMsgType0BodyReaderUnsafe<T> : MsgBodyReader<T>
//    {

//        public FindMsgType0BodyReaderUnsafe(ResponseMsgMessage message)
//            : base(null!, message)
//        {

//        }


//        public override bool TryParseMessage(
//            in ReadOnlySequence<byte> input,
//            ref SequencePosition consumed,
//            ref SequencePosition examined,
//            [MaybeNullWhen(false)] out CursorResult<T> message)
//        {
//            var bsonReader = new BsonReader(input);
//            //var a = 1;
//            //if (a == 2)
//            //{
//            //    bsonReader.TryParseDocument(out var doc);
//            //}
//            if (CursorResult<T>.TryParseBson(ref bsonReader, FirstBatch, NextBatch, out message))
//            {
//                consumed = bsonReader.Position;
//                examined = consumed;       
//                return true;
//            }
//            else
//            {
//                consumed = bsonReader.Position;
//                examined = consumed;
//                return false;
//            }
//            message = default;
//            return false;

//        }
//    }
//}
