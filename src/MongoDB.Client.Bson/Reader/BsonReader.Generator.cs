using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
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
        public unsafe bool TryReadGenericNullable<T>(BsonType bsonType, [MaybeNullWhen(false)] out T? genericValue)
        {
            genericValue = default;
            if (TryReadGeneric<T>(bsonType, out var temp))
            {
                genericValue = temp;
                return true;
            }
            return false;
        }
        public bool TryReadObject(BsonType bsonType, [MaybeNullWhen(false)] out object objectValue)
        {
            objectValue = default;
            switch (bsonType)
            {
                case BsonType.Double:
                    {
                        if (!TryGetDouble(out double doubleValue)) { return false; }
                        objectValue = doubleValue;
                        return true;
                    }
                case BsonType.String:
                    {
                        if (!TryGetString(out var stringValue)) { return false; }
                        objectValue = stringValue;
                        return true;
                    }
                case BsonType.Document:
                    {
                        if (!TryParseDocument(null, out var docValue)) { return false; }
                        objectValue = docValue;
                        return true;
                    }
                case BsonType.Array:
                    {
                        if (!TryGetArray(out var arrayDoc)) { return false; }
                        objectValue = arrayDoc;
                        return true;
                    }
                case BsonType.BinaryData:
                    {
                        if (!TryGetBinaryData(out BsonBinaryData binary)) { return false; }
                        objectValue = binary;
                        return true;
                    }
                case BsonType.ObjectId:
                    {
                        if (!TryGetObjectId(out BsonObjectId objectId)) { return false; }
                        objectValue = objectId;
                        return true;
                    }
                case BsonType.Boolean:
                    {
                        if (!TryGetBoolean(out bool boolValue)) { return false; }
                        objectValue = boolValue;
                        return true;
                    }
                case BsonType.UtcDateTime:
                    {
                        if (!TryGetUtcDatetime(out DateTimeOffset datetime)) { return false; }
                        objectValue = datetime;
                        return true;
                    }
                case BsonType.Null:
                    {
                        objectValue = null!;
                        return true;
                    }
                case BsonType.Int32:
                    {
                        if (!TryGetInt32(out int intValue)) { return false; }
                        objectValue = intValue;
                        return true;
                    }
                case BsonType.Timestamp:
                    {
                        if (!TryGetTimestamp(out BsonTimestamp timestampValue)) { return false; }
                        objectValue = timestampValue;
                        return true;
                    }
                case BsonType.Int64:
                    {
                        if (!TryGetInt64(out long longValue)) { return false; }
                        objectValue = longValue;
                        return true;
                    }
                case BsonType.Decimal:
                    {
                        if (!TryGetDecimal(out decimal decimalValue)) { return false; }
                        objectValue = decimalValue;
                        return true;
                    }
                default:
                    {
                        return ThrowHelper.UnknownTypeException<bool>((int)bsonType);
                    }
            }
        }
        public unsafe bool TryReadGeneric<T>(BsonType bsonType, [MaybeNull] out T genericValue)
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
                        if (!TryGet(bsonType, out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case bool value:
                        if (!TryGet(bsonType, out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case int value:
                        if (!TryGet(bsonType, out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case long value:
                        if (!TryGet(bsonType, out value)) { return false; }
                        genericValue = (T)(object)value;
                        return true;
                    case decimal value:
                        if (!TryGet(bsonType, out value)) { return false; }
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
                if (!TryGet(bsonType, out DateTimeOffset value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(BsonObjectId))
            {
                if (!TryGet(bsonType, out BsonObjectId value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(Guid))
            {
                if (!TryGet(bsonType, out Guid value)) { return false; }
                genericValue = (T)(object)value;
                return true;
            }
            if (typeof(T) == typeof(string))
            {
                string? strValue;
                if (!TryGet(bsonType, out strValue)) { return false; }
                genericValue = (T)(object)strValue!;
                return true;
            }
            if (typeof(T) == typeof(BsonArray))
            {
                BsonDocument? tempArray;
                if (!TryGet(bsonType, out tempArray)) { return false; }
                genericValue = (T)(object)tempArray!;
                return true;
            }
            if (typeof(T) == typeof(BsonDocument))
            {
                if (!TryGet(bsonType, out BsonDocument? value)) { return false; }
                genericValue = (T)(object)value!;
                return true;
            }
            if (typeof(T) == typeof(BsonTimestamp))
            {
                if (!TryGet(bsonType, out BsonTimestamp value)) { return false; }
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

        public bool TrySkip(BsonType bsonType)
        {
            switch (bsonType)
            {
                case BsonType.Double:
                    {
                        return TryAdvance(sizeof(double));
                    }
                case BsonType.String:
                    {
                        if (TryGetInt32(out int length))
                        {
                            return TryAdvance(length);
                        }

                        return false;
                    }
                case BsonType.Document:
                    {

                        if (TryGetInt32(out int docLength))
                        {
                            return TryAdvance(docLength - sizeof(int));
                        }
                        return false;
                    }
                case BsonType.Array:
                    {
                        if (TryGetInt32(out int arrayLength))
                        {
                            return TryAdvance(arrayLength - sizeof(int));
                        }

                        return false;
                    }
                case BsonType.BinaryData:
                    {
                        if (TryGetInt32(out int binDataLength))
                        {
                            return TryAdvance(binDataLength + 1);
                        }

                        return false;
                    }
                case BsonType.ObjectId:
                    {
                        return TryAdvance(12);
                    }
                case BsonType.Boolean:
                    {
                        return TryAdvance(1);
                    }
                case BsonType.UtcDateTime:
                    {
                        return TryAdvance(sizeof(long));
                    }
                case BsonType.Null:
                    {
                        return true;
                    }
                case BsonType.Int32:
                    {
                        return TryAdvance(sizeof(int));
                    }
                case BsonType.Timestamp:
                    {
                        return TryAdvance(sizeof(long));
                    }
                case BsonType.Int64:
                    {
                        return TryAdvance(sizeof(long));
                    }
                case BsonType.Decimal:
                    {
                        return TryAdvance(16);
                    }
                default:
                    {
                        return ThrowHelper.UnknownTypeException<bool>((int)bsonType);
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
