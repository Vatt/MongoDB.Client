﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSerializerNotFound(string typeName)
        {
            throw new SerializerNotFoundException(typeName);
        }
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowUnsupportedTypeType(string typeName)
        {
            throw new UnsupportedTypeException(typeName);
        }
        [DoesNotReturn]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
        }

        public unsafe bool TryReadGenericNullable<T>(int bsonType, [MaybeNullWhen(false)] out T? genericValue)
        {
            genericValue = default;
            if (TryReadGeneric<T>(bsonType, out var temp))
            {
                genericValue = temp;
                return true;
            }
            return false;
        }
        public bool TryReadObject(int bsonType, [MaybeNullWhen(false)] out object objectValue)
        {
            objectValue = default;
            switch (bsonType)
            {
                case 1:
                    {
                        if (!TryGetDouble(out double doubleValue)) { return false; }
                        objectValue = doubleValue;
                        return true;
                    }
                case 2:
                    {
                        if (!TryGetString(out var stringValue)) { return false; }
                        objectValue = stringValue;
                        return true;
                    }
                case 3:
                    {
                        if (!TryParseDocument(null, out var docValue)) { return false; }
                        objectValue = docValue;
                        return true;
                    }
                case 4:
                    {
                        if (!TryGetArray(out var arrayDoc)) { return false; }
                        objectValue = arrayDoc;
                        return true;
                    }
                case 5:
                    {
                        if (!TryGetBinaryData(out BsonBinaryData binary)) { return false; }
                        objectValue = binary;
                        return true;
                    }
                case 7:
                    {
                        if (!TryGetObjectId(out BsonObjectId objectId)) { return false; }
                        objectValue = objectId;
                        return true;
                    }
                case 8:
                    {
                        if (!TryGetBoolean(out bool boolValue)) { return false; }
                        objectValue = boolValue;
                        return true;
                    }
                case 9:
                    {
                        if (!TryGetUtcDatetime(out DateTimeOffset datetime)) { return false; }
                        objectValue = datetime;
                        return true;
                    }
                case 10:
                    {
                        objectValue = null!;
                        return true;
                    }
                case 16:
                    {
                        if (!TryGetInt32(out int intValue)) { return false; }
                        objectValue = intValue;
                        return true;
                    }
                case 17:
                    {
                        if (!TryGetInt64(out long timestampValue)) { return false; }
                        objectValue = timestampValue;
                        return true;
                    }
                case 18:
                    {
                        if (!TryGetInt64(out long longValue)) { return false; }
                        objectValue = longValue;
                        return true;
                    }
                case 19:
                    {
                        if (!TryGetDecimal(out decimal decimalValue)) { return false; }
                        objectValue = decimalValue;
                        return true;
                    }
                default:
                    {
                        return ThrowHelper.UnknownTypeException<bool>(bsonType);
                    }
            }
        }
        public unsafe bool TryReadGeneric<T>(int bsonType, [MaybeNullWhen(false)] out T genericValue)
        {
            genericValue = default;
            if (SerializerFnPtrProvider<T>.IsSerializable)
            {
                goto SERIALIZABLE;
            }
            if (SerializerFnPtrProvider<T>.IsSimpleBsonType)
            {
                //return SerializerFnPtrProvider<T>.TryParseSimpleBsonType(ref this, bsonType, out genericValue);
                goto SIMPLE_BSON_TYPE;
            }
            if (typeof(T).IsPrimitive)
            {
                switch (genericValue)
                {
                    case double value:
                        if (!TryGetDouble(out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case bool value:
                        if (!TryGetBoolean(out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case int value:
                        if (!TryGetInt32(out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case long value:
                        if (!TryGetInt64(out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case decimal value:
                        if (!TryGetDecimalWithBsonType(bsonType, out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    default:
                        ThrowSerializerNotFound(typeof(T).Name);
                        break;
                }
            }
        SIMPLE_BSON_TYPE:
            if (typeof(T) == typeof(DateTimeOffset))
            {
                if (!TryGetDateTimeWithBsonType(bsonType, out DateTimeOffset value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(BsonObjectId))
            {
                if (!TryGetObjectId(out BsonObjectId value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(Guid))
            {
                if (!TryGetGuidWithBsonType(bsonType, out Guid value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(string))
            {
                string? strvalue;
                if (!TryGetString(out strvalue)) { return false; }
                genericValue = (T)(object)strvalue;
                return true;
            }
            if (typeof(T) == typeof(BsonArray))
            {
                BsonDocument tempArray;
                if (!TryParseDocument(out tempArray)) { return false; }
                genericValue = (T)(object)tempArray;
                return true;
            }
            if (typeof(T) == typeof(BsonDocument))
            {
                if (!TryParseDocument(out var value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(BsonTimestamp))
            {
                if (!TryGetTimestamp(out BsonTimestamp value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
        SERIALIZABLE:
            var reader = SerializerFnPtrProvider<T>.TryParseFnPtr;
            if (reader != default)
            {
                return reader(ref this, out genericValue);
            }
            else
            {
                //if (SerializersMap.TryGetSerializer<T>(out var serializer) == false)
                //{
                //    ThrowSerializerNotFound(typeof(T).Name);
                //}

                //return serializer.TryParseBson(ref this, out genericValue);
                ThrowSerializerNotFound(typeof(T).Name);
                return false;
            }

        }

        public bool TrySkipCString()
        {
            return _input.TryAdvanceTo(EndMarker);
        }


        public bool TrySkip(int bsonType)
        {
            switch (bsonType)
            {
                case 1:
                    {
                        return TryAdvance(sizeof(double));
                    }
                case 2:
                    {
                        if (TryGetInt32(out int length))
                        {
                            return TryAdvance(length);
                        }

                        return false;
                    }
                case 3:
                    {

                        if (TryGetInt32(out int docLength))
                        {
                            return TryAdvance(docLength - sizeof(int));
                        }
                        return false;
                    }
                case 4:
                    {
                        if (TryGetInt32(out int arrayLength))
                        {
                            return TryAdvance(arrayLength - sizeof(int));
                        }

                        return false;
                    }
                case 5:
                    {
                        if (TryGetInt32(out int binDataLength))
                        {
                            return TryAdvance(binDataLength + 1);
                        }

                        return false;
                    }
                case 7:
                    {
                        return TryAdvance(12);
                    }
                case 8:
                    {
                        return TryAdvance(1);
                    }
                case 9:
                    {
                        return TryAdvance(sizeof(long));
                    }
                case 10:
                    {
                        return true;
                    }
                case 16:
                    {
                        return TryAdvance(sizeof(int));
                    }
                case 17:
                    {
                        return TryAdvance(sizeof(long));
                    }
                case 18:
                    {
                        return TryAdvance(sizeof(long));
                    }
                case 19:
                    {
                        return TryAdvance(16);
                    }
                default:
                    {
                        return ThrowHelper.UnknownTypeException<bool>(bsonType);
                    }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAdvance(long value)
        {
            if (_input.Remaining >= value)
            {
                _input.Advance(value);
                return true;
            }
            return false;
        }

        public bool TryGetDecimalWithBsonType(int bsonType, out decimal value)
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
                        if (decimal.TryParse(stringValue, CultureInfo.InvariantCulture, out value) is false)
                        {
                            return ThrowHelper.UnsupportedStringDecimalException<bool>(stringValue);
                        }

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

        public bool TryGetGuidWithBsonType(int bsonType, out Guid value)
        {
            switch (bsonType)
            {
                case 5:
                    return TryGetBinaryDataGuid(out value);
                case 2:
                    return TryGetGuidFromString(out value);
                default:
                    value = default;
                    return ThrowHelper.UnsupportedGuidTypeException<bool>(bsonType);
            }
        }


        public bool TryGetDateTimeWithBsonType(int bsonType, out DateTimeOffset value)
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


        public bool TryGetBinaryData(byte expectedSubtype, [MaybeNullWhen(false)] out byte[] value)
        {
            if (TryGetInt32(out int len))
            {
                if (_input.Remaining > len)
                {
                    TryGetByte(out var subtype);
                    if (subtype == expectedSubtype)
                    {
                        value = new byte[len];
                        if (_input.TryCopyTo(value)) //TODO: check it
                        {
                            _input.Advance(len);
                            return true;
                        }
                        value = default;
                        return false;
                    }
                    value = default;
                    return ThrowHelper.UnknownSubtypeException<bool>(subtype);
                }
            }
            value = default;
            return false;
        }


        public bool TryGetBinaryData(byte expectedSubtype, out Memory<byte> value)
        {
            if (TryGetBinaryData(expectedSubtype, out byte[]? temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }


        public bool TryGetBinaryData(byte expectedSubtype, out Memory<byte>? value)
        {
            if (TryGetBinaryData(expectedSubtype, out byte[]? temp))
            {
                value = temp;
                return true;
            }

            value = default;
            return false;
        }
    }
}
