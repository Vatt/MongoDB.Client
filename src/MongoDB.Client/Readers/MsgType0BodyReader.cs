using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MongoDB.Client.Readers
{
    internal class MsgType0BodyReader<T> : MsgBodyReader<T>
    {

        private long _modelsReaded;
        private long _payloadLength;
        private long _modelsLength;
        private long _docLength;
        private ParserState _state;

        public MsgType0BodyReader(IGenericBsonSerializer<T> serializer, ResponseMsgMessage message)
            : base(serializer, message)
        {
            _payloadLength = message.Header.MessageLength - 21; // message header + msg flags + payloadType
            _state = ParserState.Initial;
        }

        public override bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
        {
            var bsonReader = new BsonReader(input);

            if (_state == ParserState.Initial)
            {
                if (TryReadCursorStart(ref bsonReader, out var modelsLength, out var docLength, out var hasBatch) == false)
                {
                    return false;
                }
                _modelsLength = modelsLength - 4;
                _docLength = docLength;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = hasBatch ? ParserState.Models : ParserState.Complete;
            }

            if (_state == ParserState.Models)
            {
                long initialConsumed = bsonReader.BytesConsumed;
                long consumedBytes = 0;
                var items = CursorResult.Cursor.Items;
                while (_modelsReaded + consumedBytes < _modelsLength - 1)
                {
#if DEBUG
                    if (bsonReader.TryGetByte(out var type) == false) { return false; }
                    if (bsonReader.TryGetCString(out var name) == false) { return false; }
#else
                    if (bsonReader.TryGetByte(out _) == false) { return false; }
                    if (bsonReader.TryGetCStringAsSpan(out _) == false) { return false; }
#endif

                    if (Serializer.TryParse(ref bsonReader, out var item))
                    {
                        items.Add(item);
                        consumedBytes = bsonReader.BytesConsumed - initialConsumed;
                        consumed = bsonReader.Position;
                        examined = bsonReader.Position;
                    }
                    else
                    {
                        _modelsReaded += consumedBytes;
                        message = default;
                        return false;
                    }
                }
                _modelsReaded += consumedBytes;
                _state = ParserState.ModelsEnd;
            }

            if (_state == ParserState.ModelsEnd)
            {
                if (bsonReader.TryGetByte(out var endDocumentMarker) == false)
                {
                    return false;
                }
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Cursor;
            }

            if (_state == ParserState.Cursor)
            {
                if (TryReadCursorEnd(ref bsonReader) == false)
                {
                    return false;
                }

                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Complete;
            }

            Complete = _state == ParserState.Complete;

            return true;
        }

#if DEBUG
        private bool TryReadCursorStart(ref BsonReader reader, out int modelsLength, out int docLength, out bool hasBatch)
        {
            modelsLength = 0;
            hasBatch = false;
            string name;
            if (!reader.TryGetInt32(out docLength)) { return false; }
            if (TryGetName(ref reader, out name) == false) { return false; }

            if (name == "cursor")
            {
                var initConsumed = reader.BytesConsumed;
                if (!reader.TryGetInt32(out var cursorLength)) { return false; }
                bool hasItems;
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

                byte endMarker;
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

            return false;
        }

        private bool TryReadCursorEnd(ref BsonReader reader)
        {
            string name;
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

        private static bool TryGetName(ref BsonReader breader, out string name)
        {
            if (!breader.TryGetByte(out _)) { name = default; return false; }
            if (!breader.TryGetCString(out name)) { return false; }
            return true;
        }

        private bool TryParseValue(ref BsonReader reader, string name, out bool hasItems, out int modelsLength)
        {
            hasItems = false;
            modelsLength = 0;
            if (name == "id")
            {
                if (!reader.TryGetInt64(out var idValue)) { return false; }
                CursorResult.Cursor.Id = idValue;
                return true;
            }
            if (name == "ns")
            {
                if (!reader.TryGetString(out var nsValue)) { return false; }
                CursorResult.Cursor.Namespace = nsValue;
                return true;
            }
            if (name == "ok")
            {
                if (!reader.TryGetDouble(out var okValue)) { return false; }
                CursorResult.Ok = okValue;
                return true;
            }
            if (name == "firstBatch")
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

            return ThrowHelper.UnknownCursorFieldException<bool>(name);
        }
#else
        private bool TryReadCursorStart(ref BsonReader reader, out int modelsLength, out int docLength, out bool hasBatch)
        {
            modelsLength = 0;
            hasBatch = false;
            ReadOnlySpan<byte> name;
            if (!reader.TryGetInt32(out docLength)) { return false; }
            if (TryGetName(ref reader, out name) == false) { return false; }

            if (name.SequenceEqual(CursorSpan))
            {
                var initConsumed = reader.BytesConsumed;
                if (!reader.TryGetInt32(out var cursorLength)) { return false; }
                bool hasItems;
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

                byte endMarker;
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

            return false;
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
                CursorResult.Cursor.Id = idValue;
                return true;
            }
            if (name.SequenceEqual(NsSpan))
            {
                if (!reader.TryGetString(out var nsValue)) { return false; }
                CursorResult.Cursor.Namespace = nsValue;
                return true;
            }
            if (name.SequenceEqual(OkSpan))
            {
                if (!reader.TryGetDouble(out var okValue)) { return false; }
                CursorResult.Ok = okValue;
                return true;
            }
            if (name.SequenceEqual(FirstBatchSpan))
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

            return ThrowHelper.UnknownCursorFieldException<bool>(Encoding.UTF8.GetString(name));
        }
#endif


        private static ReadOnlySpan<byte> CursorSpan => new byte[] { 99, 117, 114, 115, 111, 114 }; // cursor
        private static ReadOnlySpan<byte> FirstBatchSpan => new byte[] { 102, 105, 114, 115, 116, 66, 97, 116, 99, 104 }; // firstBatch
        private static ReadOnlySpan<byte> IdSpan => new byte[] { 105, 100 }; // id
        private static ReadOnlySpan<byte> NsSpan => new byte[] { 110, 115 }; // ns
        private static ReadOnlySpan<byte> OkSpan => new byte[] { 111, 107 }; // ok


        private bool TryReadCursorStart2(ref BsonReader reader, out int modelsLength, out int docLength, out bool hasBatch)
        {
            modelsLength = 0;
            docLength = 0;
            hasBatch = false;
            if (reader.TryParseDocument(out var doc))
            {
                return true;
            }

            return false;
        }

        private enum ParserState
        {
            Initial, Models, ModelsEnd, Cursor, Complete
        }
    }
}
