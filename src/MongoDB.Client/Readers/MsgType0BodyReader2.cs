using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Readers
{
    internal class MsgType0BodyReader2<T> : MsgBodyReader<T>
    {
        public CursorOwner CursorOwner { get; private set; }
        private long _modelsReaded;
        private long _payloadLength;
        private long _modelsLength;
        private long _docLength;
        private ParserState _state;

        public MsgType0BodyReader2(IGenericBsonSerializer<T> serializer, MsgMessage message)
            : base(serializer, message)
        {
            _payloadLength = message.Header.MessageLength - 21; // message header + msg flags + payloadType
            _state = ParserState.Initial;
            CursorOwner = new CursorOwner
            {
                Cursor = new Cursor()
            };
        }

        public override bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, [MaybeNullWhen(false)] out Unit message)
        {
            var bsonReader = new MongoDBBsonReader(input);

            if (_state == ParserState.Initial)
            {
                if (TryReadCursorStart(ref bsonReader, out var modelsLength, out var docLength) == false)
                {
                    return false;
                }
                _modelsLength = modelsLength - 4;
                _docLength = docLength;
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Models;
            }

            if (_state == ParserState.Models)
            {
                long initialConsumed = bsonReader.BytesConsumed;
                long consumedBytes = 0;
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
                        Objects.Add(item);
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
                Debug.Assert(bsonReader.BytesConsumed == _docLength);
                consumed = bsonReader.Position;
                examined = bsonReader.Position;
                _state = ParserState.Complete;
            }

            Complete = _state == ParserState.Complete;

            return true;
        }


        private static bool TryReadCursorStart(ref MongoDBBsonReader reader, out int modelsLength, out int docLength)
        {
            modelsLength = 0;
            if (!reader.TryGetInt32(out docLength)) { return false; }
            if (!reader.TryGetByte(out var type)) { return false; }
            if (!reader.TryGetCStringAsSpan(out var name)) { return false; }

            if (name.SequenceEqual(CursorSpan))
            {
                if (!reader.TryGetInt32(out var docLength2)) { return false; }
                if (!reader.TryGetByte(out var type2)) { return false; }
                if (!reader.TryGetCStringAsSpan(out var name2)) { return false; }
                if (name2.SequenceEqual(FirstBatchSpan))
                {
                    if (!reader.TryGetInt32(out modelsLength)) { return false; }
                    return true;
                }
            }

            return false;
        }

        private bool TryReadCursorEnd(ref MongoDBBsonReader reader)
        {
            // TODO: spans instead of strings

            string name;
            byte endMarker = 1;
            if (TryGetName(ref reader, out name) == false) { return false; }
            if (TryParseValue(ref reader, name) == false) { return false; }

            if (TryGetName(ref reader, out name) == false) { return false; }
            if (TryParseValue(ref reader, name) == false) { return false; }

            if (!reader.TryGetByte(out endMarker)) { return false; }

            if (TryGetName(ref reader, out name) == false) { return false; }
            if (TryParseValue(ref reader, name) == false) { return false; }
            if (!reader.TryGetByte(out endMarker)) { return false; }

            return true;


            static bool TryGetName(ref MongoDBBsonReader breader, out string name)
            {
                if (!breader.TryGetByte(out _)) { name = default; return false; }
                if (!breader.TryGetCString(out name)) { return false; }
                return true;
            }

            bool TryParseValue(ref MongoDBBsonReader breader, string name)
            {
                if (name == "id")
                {
                    if (!breader.TryGetInt64(out var idValue)) { return false; }
                    CursorOwner.Cursor.Id = idValue;
                    return true;
                }
                else if (name == "ns")
                {
                    if (!breader.TryGetString(out var nsValue)) { return false; }
                    CursorOwner.Cursor.Namespace = nsValue;
                    return true;
                }
                else if (name == "ok")
                {
                    if (!breader.TryGetDouble(out var okValue)) { return false; }
                    CursorOwner.Ok = okValue;
                    return true;
                }

                return ThrowHelper.UnknownCursorFieldException<bool>(name);
            }
        }


        private static ReadOnlySpan<byte> CursorSpan => new byte[] { 99, 117, 114, 115, 111, 114 }; // cursor
        private static ReadOnlySpan<byte> FirstBatchSpan => new byte[] { 102, 105, 114, 115, 116, 66, 97, 116, 99, 104 }; // firstBatch


        private bool TryReadCursorStart2(ref MongoDBBsonReader reader, out int modelsLength, out int docLength)
        {
            modelsLength = 0;
            docLength = 0;
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
