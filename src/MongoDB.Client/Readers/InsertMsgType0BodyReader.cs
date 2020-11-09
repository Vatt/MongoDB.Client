using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Messages;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Readers
{
    internal class InsertMsgType0BodyReader : IMessageReader<InsertResult>
    {
        public int Consumed { get; set; }
        
        private readonly InsertResult _result = new InsertResult();
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed,
            ref SequencePosition examined, [MaybeNullWhen(false)] out InsertResult message)
        {
            var bsonReader = new BsonReader(input);
            message = _result;
            if (TryReadCursorStart(ref bsonReader) == false)
            {
                return false;
            }
            
            consumed = bsonReader.Position;
            examined = bsonReader.Position;
            
            Consumed = (int)bsonReader.BytesConsumed;

            return true;
        }

#if DEBUG
        private bool TryReadCursorStart(ref BsonReader reader)
        {
            string name;
            byte endMarker;
            var checkpoint = reader.BytesConsumed;
            if (!reader.TryGetInt32(out var docLength))
            {
                return false;
            }

            do
            {
                if (TryGetName(ref reader, out name) == false)
                {
                    return false;
                }


                if (name == "ok")
                {
                    if (!reader.TryGetDouble(out var okValue))
                    {
                        return false;
                    }

                    _result.Ok = okValue;
                }

                if (name == "n")
                {
                    if (!reader.TryGetInt32(out var okValue))
                    {
                        return false;
                    }

                    _result.N = okValue;
                }
                
                if (name == "errmsg")
                {
                    if (!reader.TryGetString(out var errorValue))
                    {
                        return false;
                    }

                    _result.ErrorMessage = errorValue;
                }

                if (name == "code")
                {
                    if (!reader.TryGetInt32(out var codeValue))
                    {
                        return false;
                    }

                    _result.Code = codeValue;
                }

                if (name == "codeName")
                {
                    if (!reader.TryGetString(out var codeNameValue))
                    {
                        return false;
                    }

                    _result.CodeName = codeNameValue;
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

        private static bool TryGetName(ref BsonReader reader, out string name)
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
#else
        private bool TryReadCursorStart(ref BsonReader reader)
        {
            ReadOnlySpan<byte> name;
            byte endMarker;
            bool hasItems;
            var checkpoint = reader.BytesConsumed;
            if (!reader.TryGetInt32(out var docLength))
            {
                return false;
            }

            do
            {
                if (TryGetName(ref reader, out name) == false)
                {
                    return false;
                }

                if (name.SequenceEqual(OkSpan))
                {
                    if (!reader.TryGetDouble(out var okValue))
                    {
                        return false;
                    }

                    _result.Ok = okValue;
                }
                
                if (name.SequenceEqual(NSpan))
                {
                    if (!reader.TryGetInt32(out var okValue))
                    {
                        return false;
                    }

                    _result.N = okValue;
                }

                if (name.SequenceEqual(ErrmsgSpan))
                {
                    if (!reader.TryGetString(out var errorValue))
                    {
                        return false;
                    }

                    _result.ErrorMessage = errorValue;
                }

                if (name.SequenceEqual(CodeSpan))
                {
                    if (!reader.TryGetInt32(out var codeValue))
                    {
                        return false;
                    }

                    _result.Code = codeValue;
                }

                if (name.SequenceEqual(CodeNameSpan))
                {
                    if (!reader.TryGetString(out var codeNameValue))
                    {
                        return false;
                    }

                    _result.CodeName = codeNameValue;
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

        private static bool TryGetName(ref BsonReader reader, out ReadOnlySpan<byte> name)
        {
            if (!reader.TryGetByte(out _))
            {
                name = default;
                return false;
            }

            if (!reader.TryGetCStringAsSpan(out name))
            {
                return false;
            }

            return true;
        }
#endif
        
        private static ReadOnlySpan<byte> NSpan => new byte[] {110}; // n
        private static ReadOnlySpan<byte> OkSpan => new byte[] {111, 107}; // ok
        private static ReadOnlySpan<byte> ErrmsgSpan => new byte[] {101, 114, 114, 109, 115, 103}; // errmsg
        private static ReadOnlySpan<byte> CodeSpan => new byte[] {99, 111, 100, 101}; // code
        private static ReadOnlySpan<byte> CodeNameSpan => new byte[] {99, 111, 100, 101, 78, 97, 109, 101}; // codeName
    }
}