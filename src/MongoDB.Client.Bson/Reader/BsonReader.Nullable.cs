using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {

        public bool TryGetDateTimeWithBsonType(int bsonType, out DateTimeOffset? value)
        {
            switch (bsonType)
            {
                case 3:
                    return TryGetDatetimeFromDocument(out value);
                case 9:
                    return TryGetUtcDatetime(out value);
                case 18:
                    return TryGetUtcDatetime(out value);
                default:
                    value = default;
                    return ThrowHelper.UnsupportedDateTimeTypeException<bool>(bsonType);
            }
        }
        public bool TryGetDecimalWithBsonType(int bsonType, out decimal? value)
        {
            value = default;
            switch (bsonType)
            {
                case 1:
                    if (TryGetDouble(out double doubleValue))
                    {
                        value = new(doubleValue);

                        return true;
                    }

                    return false;
                case 2:
                    if (TryGetString(out var stringValue))
                    {
                        if (decimal.TryParse(stringValue, CultureInfo.InvariantCulture, out var temp) is false)
                        {
                            return ThrowHelper.UnsupportedStringDecimalException<bool>(stringValue);
                        }

                        value = temp;
                        return true;
                    }

                    return false;
                case 16:
                    if (TryGetInt32(out int intValue))
                    {
                        value = new(intValue);

                        return true;
                    }

                    return false;
                case 18:
                    if (TryGetInt64(out long longValue))
                    {
                        value = new(longValue);

                        return true;
                    }

                    return false;
                case 19:
                    return TryGetDecimal(out value);
                default:
                    value = default;
                    return ThrowHelper.UnsupportedDecimalTypeException<bool>(bsonType);
            }
        }

        public bool TryGetUtcDatetime([MaybeNullWhen(false)] out DateTimeOffset? value)
        {
            if (TryGetInt64(out long data))
            {
                value = DateTimeOffset.FromUnixTimeMilliseconds(data);
                return true;
            }

            value = default;
            return false;
        }
        public bool TryGetDatetimeFromDocument(out DateTimeOffset? date)
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

        public bool TryGetObjectId(out BsonObjectId? value)
        {
            const int oidSize = 12;

            if (_input.UnreadSpan.Length >= oidSize)
            {
                value = new BsonObjectId(_input.UnreadSpan);
                _input.Advance(oidSize);
                return true;
            }
            if (_input.Remaining >= oidSize)
            {
                return SlowGetObjectId(out value);
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool SlowGetObjectId(out BsonObjectId? value)
        {
            const int oidSize = 12;
            Span<byte> buffer = stackalloc byte[oidSize];
            if (_input.TryCopyTo(buffer))
            {
                value = new BsonObjectId(buffer);
                _input.Advance(oidSize);
                return true;
            }
            value = default;
            return false;
        }


        public bool TryGetGuidWithBsonType(int bsonType, out Guid? value)
        {
            if (bsonType == 5)
            {
                return TryGetBinaryDataGuid(out value);
            }
            if (bsonType == 2)
            {
                return TryGetGuidFromString(out value);
            }

            value = default;
            return ThrowHelper.UnsupportedGuidTypeException<bool>(bsonType);
        }
        public bool TryGetBinaryDataGuid(out Guid? value)
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
        public bool TryGetBinaryDataGuidSlow(int len, out Guid? value)
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



        public bool TryGetGuidFromString(out Guid? value)
        {
            if (TryGetString(out var data))
            {
                value = new Guid(data);
                return true;
            }
            value = default;
            return false;
        }


        public bool TryGetInt32(out int? value)
        {
            if (_input.TryReadLittleEndian(out int temp))
            {
                value = temp;
                return true;
            }
            value = default;
            return false;
        }



        public bool TryGetInt64(out long? value)
        {
            if (_input.TryReadLittleEndian(out long temp))
            {
                value = temp;
                return true;
            }
            value = default;
            return false;
        }


        public bool TryGetDouble(out double? value)
        {
            if (TryGetInt64(out long temp))
            {
                value = BitConverter.Int64BitsToDouble(temp);
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetDecimal(out decimal? value)
        {
            const int decimalSize = 16;

            if (_input.Remaining >= decimalSize)
            {
                Span<byte> span = stackalloc byte[decimalSize];
                if (_input.TryCopyTo(span))
                {
                    var bits = MemoryMarshal.Cast<byte, int>(span);
                    value = new(bits);
                    _input.Advance(decimalSize);

                    return true;
                }
            }

            value = default;
            return false;
        }

        public bool TryGetBoolean(out bool? value)
        {
            if (TryGetByte(out var boolean))
            {
                value = boolean == 1;
                return true;

            }

            value = default;
            return false;
        }
    }
}
