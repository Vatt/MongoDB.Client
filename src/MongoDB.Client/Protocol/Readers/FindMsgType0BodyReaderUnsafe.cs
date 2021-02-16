using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Utils;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Protocol.Readers
{
    internal class FindMsgType0BodyReaderUnsafe<T> : IMessageReader<CursorResult<T>>
    {
        private long _modelsReaded;
        private long _payloadLength;
        private long _modelsLength;
        private long _docLength;

        private ParserState _state;

        private CursorResult<T> _cursorResult;
        private ResponseMsgMessage Message;
        public bool Complete { get; protected set; }


        protected long _readed;
        public long Readed => _readed;
        public FindMsgType0BodyReaderUnsafe(ResponseMsgMessage message)
        {
            Message = message;
            _cursorResult = new CursorResult<T>
            {
                MongoCursor = new MongoCursor<T>
                {
                    Items = ListsPool<T>.Pool.Get()
                }
            };
            _payloadLength = message.Header.MessageLength;
            _state = ParserState.Initial;
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed,
            ref SequencePosition examined, [MaybeNullWhen(false)] out CursorResult<T> message)
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

                _readed += bsonReader.BytesConsumed - checkpoint;
                _modelsLength = modelsLength - 4;
                _docLength = docLength;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
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
                    // if (bsonReader.TryGetByte(out _) == false) { return false; }
                    // if (bsonReader.TryGetCStringAsSpan(out _) == false) { return false; }
                    if (bsonReader.TryAdvanceTo(0) == false)
                    {
                        return false;
                    }
#endif
                    if (bsonReader.TryPeekInt32(out int modelLength) && bsonReader.Remaining >= modelLength)
                    {
                        bool tryParseResult = default;
                        T item = default;
                        unsafe
                        {
                            //TODO: FIX IT
                            tryParseResult = SerializerFnPtrProvider<T>.TryParseFnPtr(ref bsonReader, out item);
                        }
                        if (tryParseResult)
                        {
                            items.Add(item);
                            _modelsReaded += bsonReader.BytesConsumed - checkpoint;
                            _readed += bsonReader.BytesConsumed - checkpoint;
                            consumed = bsonReader.Position;
                            examined = bsonReader.Position;
                        }
                        else
                        {
                            message = default;
                            return false;
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

                _readed += 1;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Cursor;
            }

            if (_state == ParserState.Cursor)
            {
                var checkpoint = bsonReader.BytesConsumed;
                if (TryReadCursorEnd(ref bsonReader) == false)
                {
                    return false;
                }

                _readed += bsonReader.BytesConsumed - checkpoint;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Complete;
            }

            Debug.Assert(_docLength == _readed);
            Debug.Assert(_readed == _payloadLength - 21); // message header + msg flags + payloadType;

            Complete = _state == ParserState.Complete;

            return true;
        }

#if DEBUG
        private bool TryReadCursorStart(ref BsonReader reader, out int modelsLength, out int docLength,
            out bool hasBatch)
        {
            modelsLength = 0;
            hasBatch = false;
            string? name;
            byte endMarker;
            bool hasItems;
            var checkpoint = reader.BytesConsumed;
            if (!reader.TryGetInt32(out docLength))
            {
                return false;
            }

            do
            {
                if (TryGetName(ref reader, out name) == false)
                {
                    return false;
                }

                if (name == "cursor")
                {
                    var initConsumed = reader.BytesConsumed;
                    if (!reader.TryGetInt32(out var cursorLength))
                    {
                        return false;
                    }

                    while (reader.BytesConsumed - initConsumed < cursorLength - 1)
                    {
                        if (TryGetName(ref reader, out name) == false)
                        {
                            return false;
                        }

                        if (TryParseValue(ref reader, name, out hasItems, out modelsLength) == false)
                        {
                            return false;
                        }

                        if (hasItems)
                        {
                            hasBatch = true;
                            return true;
                        }
                    }


                    if (!reader.TryGetByte(out endMarker))
                    {
                        return false;
                    }

                    if (endMarker is 0)
                    {
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

                    return ThrowHelper.MissedDocumentEndMarkerException<bool>();
                }


                if (name == "ok")
                {
                    if (!reader.TryGetDouble(out var okValue))
                    {
                        return false;
                    }

                    _cursorResult.Ok = okValue;
                }

                if (name == "errmsg")
                {
                    if (!reader.TryGetString(out var errorValue))
                    {
                        return false;
                    }

                    _cursorResult.ErrorMessage = errorValue;
                }

                if (name == "code")
                {
                    if (!reader.TryGetInt32(out var codeValue))
                    {
                        return false;
                    }

                    _cursorResult.Code = codeValue;
                }

                if (name == "codeName")
                {
                    if (!reader.TryGetString(out var codeNameValue))
                    {
                        return false;
                    }

                    _cursorResult.CodeName = codeNameValue;
                }
            } while (reader.BytesConsumed - checkpoint < docLength - 1);

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

        private bool TryReadCursorEnd(ref BsonReader reader)
        {
            string? name;
            if (TryGetName(ref reader, out name) == false)
            {
                return false;
            }

            if (TryParseValue(ref reader, name, out _, out _) == false)
            {
                return false;
            }

            if (TryGetName(ref reader, out name) == false)
            {
                return false;
            }

            if (TryParseValue(ref reader, name, out _, out _) == false)
            {
                return false;
            }

            byte endMarker;
            if (!reader.TryGetByte(out endMarker))
            {
                return false;
            }

            if (endMarker is 0)
            {
                if (TryGetName(ref reader, out name) == false)
                {
                    return false;
                }

                if (TryParseValue(ref reader, name, out _, out _) == false)
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

            return ThrowHelper.MissedDocumentEndMarkerException<bool>();
        }

        private static bool TryGetName(ref BsonReader reader, [MaybeNullWhen(false)] out string name)
        {
            if (!reader.TryGetByte(out _))
            {
                name = default;
                return false;
            }

            if (!reader.TryGetCString(out name))
            {
                return false;
            }

            return true;
        }

        private bool TryParseValue(ref BsonReader reader, string name, out bool hasItems, out int modelsLength)
        {
            hasItems = false;
            modelsLength = 0;
            if (name == "id")
            {
                if (!reader.TryGetInt64(out var idValue))
                {
                    return false;
                }

                _cursorResult.MongoCursor.Id = idValue;
                return true;
            }

            if (name == "ns")
            {
                if (!reader.TryGetString(out var nsValue))
                {
                    return false;
                }

                _cursorResult.MongoCursor.Namespace = nsValue;
                return true;
            }

            if (name == "ok")
            {
                if (!reader.TryGetDouble(out var okValue))
                {
                    return false;
                }

                _cursorResult.Ok = okValue;
                return true;
            }

            if (name == "firstBatch" || name == "nextBatch")
            {
                if (!reader.TryGetInt32(out modelsLength))
                {
                    return false;
                }

                if (modelsLength == 5)
                {
                    if (!reader.TryGetByte(out var endMarker))
                    {
                        return false;
                    }

                    return true;
                }
                else
                {
                    hasItems = true;
                    return true;
                }
            }

            return ThrowHelper.UnknownCursorFieldException<bool>(name);
        }
#else
        private bool TryReadCursorStart(ref BsonReader reader, out int modelsLength, out int docLength, out bool hasBatch)
        {
            modelsLength = 0;
            hasBatch = false;
            ReadOnlySpan<byte> name;
            byte endMarker;
            bool hasItems;
            var checkpoint = reader.BytesConsumed;
            if (!reader.TryGetInt32(out docLength)) { return false; }
            do
            {

                if (TryGetName(ref reader, out name) == false) { return false; }
                if (name.SequenceEqual(CursorSpan))
                {
                    var initConsumed = reader.BytesConsumed;
                    if (!reader.TryGetInt32(out var cursorLength)) { return false; }

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


                    if (!reader.TryGetByte(out endMarker)) { return false; }
                    if (endMarker is 0)
                    {
                        if (TryGetName(ref reader, out name) == false) { return false; }
                        if (TryParseValue(ref reader, name, out _, out modelsLength) == false) { return false; }
                        if (!reader.TryGetByte(out endMarker)) { return false; }
                        if (endMarker is 0)
                        {
                            return true;
                        }
                    }

                    return ThrowHelper.MissedDocumentEndMarkerException<bool>();
                }


                if (name.SequenceEqual(OkSpan))
                {
                    if (!reader.TryGetDouble(out var okValue)) { return false; }
                    _cursorResult.Ok = okValue;
                    continue;
                }

                if (name.SequenceEqual(ErrorMessageSpan))
                {
                    if (!reader.TryGetString(out var errorValue)) { return false; }
                    _cursorResult.ErrorMessage = errorValue;
                    continue;
                }

                if (name.SequenceEqual(CodeSpan))
                {
                    if (!reader.TryGetInt32(out var codeValue)) { return false; }
                    _cursorResult.Code = codeValue;
                    continue;
                }

                if (name.SequenceEqual(CodeNameSpan))
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

        private bool TryReadCursorEnd(ref BsonReader reader)
        {
            ReadOnlySpan<byte> name;
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
                if (!reader.TryGetByte(out endMarker)) { return false; }
                if (endMarker is 0)
                {
                    return true;
                }
            }

            return ThrowHelper.MissedDocumentEndMarkerException<bool>();
        }

        private static bool TryGetName(ref BsonReader breader, out ReadOnlySpan<byte> name)
        {
            if (!breader.TryGetByte(out _)) { name = default; return false; }
            if (!breader.TryGetCStringAsSpan(out name)) { return false; }
            return true;
        }

        private bool TryParseValue(ref BsonReader reader, ReadOnlySpan<byte> name, out bool hasItems, out int modelsLength)
        {
            hasItems = false;
            modelsLength = 0;
            if (name.SequenceEqual(IdSpan))
            {
                if (!reader.TryGetInt64(out var idValue)) { return false; }
                _cursorResult.MongoCursor.Id = idValue;
                return true;
            }
            if (name.SequenceEqual(NsSpan))
            {
                if (!reader.TryGetString(out var nsValue)) { return false; }
                _cursorResult.MongoCursor.Namespace = nsValue;
                return true;
            }
            if (name.SequenceEqual(OkSpan))
            {
                if (!reader.TryGetDouble(out var okValue)) { return false; }
                _cursorResult.Ok = okValue;
                return true;
            }
            if (name.SequenceEqual(FirstBatchSpan) || name.SequenceEqual(NextBatchSpan))
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

            return ThrowHelper.UnknownCursorFieldException<bool>(System.Text.Encoding.UTF8.GetString(name));
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