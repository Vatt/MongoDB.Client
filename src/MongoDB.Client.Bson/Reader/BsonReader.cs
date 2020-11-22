using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;
using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        private const byte EndMarker = (byte) '\x00';


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

        
        public bool TryAdvanceTo(byte delimiter, bool advancePastDelimiter = true)
        {
            return _input.TryAdvanceTo(delimiter, advancePastDelimiter);
        }


        public bool TryGetByte(out byte value)
        {
            return _input.TryRead(out value);
        }


        public bool TryGetInt16(out short value)
        {
            return _input.TryReadLittleEndian(out value);
        }


        public bool TryGetInt32(out int value)
        {
            return _input.TryReadLittleEndian(out value);
        }

        public bool TryPeekInt32(out int value)
        {
            if (_input.TryReadLittleEndian(out value))
            {
                _input.Rewind(sizeof(int));
                return true;
            }
            return false;
        }

        public bool TryGetInt64(out long value)
        {
            return _input.TryReadLittleEndian(out value);
        }


        public bool TryGetDouble(out double value)
        {
            value = default;
            if (!TryGetInt64(out var temp))
            {
                return false;
            }

            value = BitConverter.Int64BitsToDouble(temp);
            return true;
        }

        public bool TryGetCString([MaybeNullWhen(false)] out string value)
        {
            value = default;
            if (!_input.TryReadTo(out ReadOnlySpan<byte> data, EndMarker))
            {
                return false;
            }

            value = Encoding.UTF8.GetString(data);
            return true;
        }


        public bool TryGetCStringAsSpan(out ReadOnlySpan<byte> value)
        {
            if (!_input.TryReadTo(out value, EndMarker))
            {
                return false;
            }

            return true;
        }


        public bool TryGetString([MaybeNullWhen(false)] out string value)
        {
            value = default;
            if (!TryGetInt32(out var length))
            {
                return false;
            }

            if (_input.Remaining < length)
            {
                return false;
            }

            var stringLength = length - 1;
            if (_input.UnreadSpan.Length >= length)
            {
                var data = _input.UnreadSpan.Slice(0, stringLength);
                _input.Advance(length);
                value = Encoding.UTF8.GetString(data);
                return true;
            }

            return SlowTryGetString(stringLength, out value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SlowTryGetString(int stringLength, [MaybeNullWhen(false)] out string? value)
        {
            value = default;
            byte[]? buffer = null;
            Span<byte> span = stringLength < 512
                ? stackalloc byte[stringLength]
                : (buffer = ArrayPool<byte>.Shared.Rent(stringLength)).AsSpan(0, stringLength);
            try
            {
                if (_input.TryCopyTo(span))
                {
                    _input.Advance(stringLength + 1);
                    value = Encoding.UTF8.GetString(span);
                    return true;
                }

                return false;
            }
            finally
            {
                if (buffer is not null)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }

        public bool TryGetStringAsSpan(out ReadOnlySpan<byte> value)
        {
            value = default;
            if (!TryGetInt32(out var length))
            {
                return false;
            }

            if (_input.Remaining < length)
            {
                return false;
            }


            if (_input.UnreadSpan.Length >= length)
            {
                value = _input.UnreadSpan.Slice(0, length - 1);
                _input.Advance(length);
                return true;
            }

            var result = new byte[length - 1];
            if (_input.TryCopyTo(result))
            {
                _input.Advance(length);
                value = result;
                return true;
            }

            return false;
        }


        public bool TryGetObjectId(out BsonObjectId value)
        {
            const int oidSize = 12;
            value = default;
            if (_input.Remaining < oidSize)
            {
                return false;
            }

            if (_input.UnreadSpan.Length >= oidSize)
            {
                value = new BsonObjectId(_input.UnreadSpan);
                _input.Advance(oidSize);
                return true;
            }

            SlowGetObjectId(out value);
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowGetObjectId(out BsonObjectId value)
        {
            const int oidSize = 12;
            Span<byte> buffer = stackalloc byte[oidSize];
            _input.TryCopyTo(buffer);
            value = new BsonObjectId(buffer);
            _input.Advance(oidSize);
        }

        public bool TryGetBinaryData(out BsonBinaryData value)
        {
            value = default;
            if (!TryGetInt32(out var len))
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
                case 0:
                {
                    var data = new byte[len];
                    if (_input.TryCopyTo(data))
                    {
                        value = BsonBinaryData.Create(data);
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
            value = default;
            if (!TryGetInt32(out var len))
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

            if (subtype == 4)
            {
                if (_input.UnreadSpan.Length < len)
                {
                    value = new Guid(_input.UnreadSpan.Slice(0, len));
                    _input.Advance(len);
                    return true;
                }

                Span<byte> buffer = stackalloc byte[len];
                if (_input.TryCopyTo(buffer))
                {
                    value = new Guid(buffer);
                    _input.Advance(len);
                    return true;
                }

                return false;
            }


            return ThrowHelper.UnknownSubtypeException<bool>(subtype);
        }


        public bool TryGetGuidFromString(out Guid value)
        {
            value = default;
            if (TryGetString(out var data))
            {
                value = new Guid(data);
            }

            return true;
        }


        public bool TryGetUtcDatetime([MaybeNullWhen(false)] out DateTimeOffset value)
        {
            value = default;
            if (!TryGetInt64(out var data))
            {
                return false;
            }

            value = DateTimeOffset.FromUnixTimeMilliseconds(data);
            return true;
        }


        public bool TryGetBoolean(out bool value)
        {
            value = default;
            if (!TryGetByte(out var boolean))
            {
                return false;
            }

            value = boolean == 1;
            return true;
        }


        public bool TryGetDatetimeFromDocument(out DateTimeOffset date)
        {
            date = default;
            if (!TryGetInt32(out var docLength))
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

            if (!TryGetInt64(out var longDate))
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

            if (!TryGetInt64(out var ticks))
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

            if (!TryGetInt32(out var offset))
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