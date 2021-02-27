using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Exceptions;
using MongoDB.Client.Bson.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
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
        SERIALIZABLE:
            var reader = SerializerFnPtrProvider<T>.TryParseFnPtr;
            if (reader != default)
            {
                return reader(ref this, out genericValue);
            }
            else
            {
                if (SerializersMap.TryGetSerializer<T>(out var serializer) == false)
                {
                    ThrowSerializerNotFound(typeof(T).Name);
                }
                
                return serializer.TryParseBson(ref this, out genericValue);
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetGuidWithBsonType(int bsonType, out Guid value)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDateTimeWithBsonType(int bsonType, out DateTimeOffset value)
        {
            if (bsonType == 3)
            {
                return TryGetDatetimeFromDocument(out value);
            }
            if (bsonType == 9)
            {
                return TryGetUtcDatetime(out value);
            }
            if (bsonType == 18)
            {
                return TryGetUtcDatetime(out value);
            }

            value = default;
            return ThrowHelper.UnsupportedDateTimeTypeException<bool>(bsonType);
        }
    }
}
