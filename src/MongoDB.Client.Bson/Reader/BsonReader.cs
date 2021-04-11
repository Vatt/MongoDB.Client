using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        private const byte EndMarker = (byte)'\x00';


        private SequenceReader<byte> _input;

        public long BytesConsumed => _input.Consumed;

        public SequencePosition Position => _input.Position;

        public readonly long Remaining => _input.Remaining;

        public BsonReader(in ReadOnlyMemory<byte> memory)
        {
            var ros = new ReadOnlySequence<byte>(memory);
            _input = new SequenceReader<byte>(ros);
        }

        public BsonReader(in ReadOnlySequence<byte> sequence)
        {
            _input = new SequenceReader<byte>(sequence);
        }

        public BsonReader(in SequenceReader<byte> reader)
        {
            _input = reader;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvanceTo(byte delimiter, bool advancePastDelimiter = true)
        {
            return _input.TryAdvanceTo(delimiter, advancePastDelimiter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetByte(out byte value)
        {
            return _input.TryRead(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInt16(out short value)
        {
            return _input.TryReadLittleEndian(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInt32(out int value)
        {
            return _input.TryReadLittleEndian(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekInt32(out int value)
        {
            if (_input.TryReadLittleEndian(out value))
            {
                _input.Rewind(sizeof(int));
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeekByte(out byte value)
        {
            if (_input.TryRead(out value))
            {
                _input.Rewind(sizeof(byte));
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetInt64(out long value)
        {
            return _input.TryReadLittleEndian(out value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDouble(out double value)
        {
            if (TryGetInt64(out long temp))
            {
                value = BitConverter.Int64BitsToDouble(temp);
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCString([MaybeNullWhen(false)] out string value)
        {
            if (_input.TryReadTo(out ReadOnlySequence<byte> data, EndMarker))
            {
                value = Encoding.UTF8.GetString(data);
                return true;
            }
            value = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCStringAsSpan(out ReadOnlySpan<byte> value)
        {
            if (_input.TryReadTo(out value, EndMarker))
            {
                return true;
            }

            return false;
        }


        public bool TryGetString([MaybeNullWhen(false)] out string value)
        {
            if (TryGetInt32(out int length))
            {
                var stringLength = length - 1;
                if (_input.UnreadSpan.Length >= stringLength)
                {
                    var data = _input.UnreadSpan.Slice(0, stringLength);
                    value = Encoding.UTF8.GetString(data);
                    _input.Advance(length);
                    return true;
                }

                return SlowTryGetString(stringLength, out value);
            }
            value = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SlowTryGetString(int stringLength, [MaybeNullWhen(false)] out string value)
        {
            if (_input.Remaining >= stringLength)
            {
                var data = _input.UnreadSequence.Slice(0, stringLength);
                _input.Advance(stringLength + 1);
                value = Encoding.UTF8.GetString(data);
                return true;
            }

            value = default;
            return false;
        }


        public bool TryGetStringAsSpan(out ReadOnlySpan<byte> value)
        {
            if (TryGetInt32(out int length))
            {
                var stringLength = length - 1;
                if (_input.UnreadSpan.Length >= stringLength)
                {
                    value = _input.UnreadSpan.Slice(0, stringLength);
                    _input.Advance(length);
                    return true;
                }

                return TryGetStringAsSpanSlow(stringLength, out value);
            }

            value = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryGetStringAsSpanSlow(int stringLength, out ReadOnlySpan<byte> value)
        {
            if (_input.Remaining >= stringLength)
            {
                var result = new byte[stringLength];
                if (_input.TryCopyTo(result))
                {
                    _input.Advance(stringLength);
                    value = result;
                    return true;
                }
            }

            value = default;
            return false;
        }


        public bool TryGetObjectId(out BsonObjectId value)
        {
            const int oidSize = 12;

            if (_input.UnreadSpan.Length >= oidSize)
            {
                value = new BsonObjectId(_input.UnreadSpan);
                _input.Advance(oidSize);
                return true;
            }

            return SlowGetObjectId(out value);
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SlowGetObjectId(out BsonObjectId value)
        {
            const int oidSize = 12;
            if (_input.Remaining >= oidSize)
            {
                Span<byte> buffer = stackalloc byte[oidSize];
                if (_input.TryCopyTo(buffer))
                {
                    value = new BsonObjectId(buffer);
                    _input.Advance(oidSize);
                    return true;
                }
            }

            value = default;
            return false;
        }


        public bool TryGetBinaryData(out BsonBinaryData value)
        {
            value = default;
            if (!TryGetInt32(out int len))
            {
                return false;
            }

            if (!TryGetByte(out var subtype))
            {
                return false;
            }

            if (_input.Remaining < len)
            {
                return false;
            }

            switch (subtype)
            {
                //Generic or MD5 binary data 
                case 0 or 5:
                    {
                        var data = new byte[len];
                        if (_input.TryCopyTo(data))
                        {
                            value = BsonBinaryData.Create((BsonBinaryDataType)subtype, data);
                            _input.Advance(len);
                            return true;
                        }

                        return false;
                    }
                case 4:
                    {
                        if (_input.UnreadSpan.Length < len)
                        {
                            value = BsonBinaryData.Create(new Guid(_input.UnreadSpan.Slice(0, len)));
                            _input.Advance(len);
                            return true;
                        }

                        Span<byte> buffer = stackalloc byte[len];
                        if (_input.TryCopyTo(buffer))
                        {
                            value = BsonBinaryData.Create(new Guid(buffer));
                            _input.Advance(len);
                            return true;
                        }

                        return false;
                    }
                default:
                    {
                        return ThrowHelper.UnknownSubtypeException<bool>(subtype);
                    }
            }
        }


        public bool TryGetBinaryDataGuid(out Guid value)
        {
            if (TryGetInt32(out int len))
            {
                if (_input.Remaining > len)
                {
                    TryGetByte(out var subtype);
                    if (subtype == 4)
                    {
                        if (_input.UnreadSpan.Length >= len)
                        {
                            value = new Guid(_input.UnreadSpan.Slice(0, len));
                            _input.Advance(len);
                            return true;
                        }
                        return TryGetBinaryDataGuidSlow(len, out value);
                    }

                    value = default;
                    return ThrowHelper.UnknownSubtypeException<bool>(subtype);
                }
            }

            value = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryGetBinaryDataGuidSlow(int len, out Guid value)
        {
            Span<byte> buffer = stackalloc byte[len];
            if (_input.TryCopyTo(buffer))
            {
                value = new Guid(buffer);
                _input.Advance(len);
                return true;
            }
            value = default;
            return false;
        }


        public bool TryGetGuidFromString(out Guid value)
        {
            if (TryGetStringAsSpan(out var data))
            {
                return Utf8Parser.TryParse(data, out value, out var consumed);
            }
            value = default;
            return false;
        }


        public bool TryGetUtcDatetime([MaybeNullWhen(false)] out DateTimeOffset value)
        {
            if (TryGetInt64(out long data))
            {
                value = DateTimeOffset.FromUnixTimeMilliseconds(data);
                return true;
            }

            value = default;
            return false;
        }


        public bool TryGetTimestamp([MaybeNullWhen(false)] out BsonTimestamp value)
        {
            if (TryGetInt64(out long data))
            {
                value = new BsonTimestamp(data);
                return true;
            }

            value = default;
            return false;
        }


        public bool TryGetBoolean(out bool value)
        {
            if (TryGetByte(out var boolean))
            {
                value = boolean == 1;
                return true;

            }

            value = default;
            return false;
        }


        public bool TryGetDatetimeFromDocument(out DateTimeOffset date)
        {
            date = default;
            if (!TryGetInt32(out int docLength))
            {
                return false;
            }

            if (!TryGetByte(out var typeDate))
            {
                return false;
            }

            if (!TryGetCString(out var nameDate))
            {
                return false;
            }

            if (!TryGetInt64(out long longDate))
            {
                return false;
            }

            if (!TryGetByte(out var typeTicks))
            {
                return false;
            }

            if (!TryGetCString(out var nameTicks))
            {
                return false;
            }

            if (!TryGetInt64(out long ticks))
            {
                return false;
            }

            if (!TryGetByte(out var typeOffset))
            {
                return false;
            }

            if (!TryGetCString(out var nameOffset))
            {
                return false;
            }

            if (!TryGetInt32(out int offset))
            {
                return false;
            }

            if (!TryGetByte(out var endDocumentMarker))
            {
                return false;
            }

            if (endDocumentMarker != EndMarker)
            {
                return ThrowHelper.MissedDocumentEndMarkerException<bool>();
            }

            date = DateTimeOffset.FromUnixTimeMilliseconds(longDate);
            return true;
        }
    }
}
