using System;
using System.Runtime.CompilerServices;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Serialization.Exceptions;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Reader
{
    public ref partial struct BsonReader
    {
        private delegate bool TryParseDelegate<T>(ref BsonReader reader, out T message);
        
        private static void ThrowSerializerNotFound(string typeName)
        {
            throw new SerializerNotFoundException(typeName);
        }
        private static void ThrowSerializerIsNull(string typeName)
        {
            throw new SerializerIsNullException(typeName);
        }
        public bool TryReadGeneric<T>(int bsonType, out T genericValue)
        {
            genericValue = default;
            switch (genericValue)
            {
                case double value:
                    if (!TryGetDouble(out value)) { return false; }
                    genericValue = (T)(object)value;
                    return true;
                case string value:
                    if (!TryGetString(out value)) { return false; }
                    genericValue = (T)(object)value;
                    return true;
                case BsonArray value:
                    BsonDocument tempArray = value;
                    if (!TryParseDocument(out tempArray)) { return false; }
                    genericValue = (T)(object)tempArray;
                    return true;
                case BsonDocument value:
                    if (!TryParseDocument(out value)) { return false; }
                    genericValue = (T)(object)value;
                    return true;
                case Guid value:
                    if (!TryGetGuidWithBsonType(bsonType, out value)) { return false; }
                    genericValue = (T)(object)value;
                    return true;
                case BsonObjectId value:
                    if (!TryGetObjectId(out value)) { return false; }
                    genericValue = (T)(object)value;
                    return true;
                case bool value:
                    if (!TryGetBoolean(out value)) { return false; }
                    genericValue = (T)(object)value;
                    return true;
                case DateTimeOffset value:
                    if (!TryGetDateTimeWithBsonType(bsonType, out value)) { return false; }
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
            }
            if (typeof(T).GetCustomAttributes(typeof(BsonSerializableAttribute), false).Length > 0)
            {
                var writer = typeof(T).GetMethod("TryParse").CreateDelegate(typeof(TryParseDelegate<T>)) as TryParseDelegate<T>;
                if (writer is null)
                {
                    ThrowSerializerNotFound(typeof(T).Name);
                }
                return writer!(ref this, out genericValue);
            }
            else
            {
                if (!SerializersMap.TryGetSerializer<T>(out var serializer))
                {
                    ThrowSerializerNotFound(typeof(T).Name);
                }

                if (serializer is null)
                {
                    ThrowSerializerIsNull(typeof(T).Name);
                }
                return serializer.TryParse(ref this, out genericValue);
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
                        if (TryGetInt32(out var length))
                        {
                            return TryAdvance(length);
                        }

                        return false;
                    }
                case 3:
                    {

                        if (TryGetInt32(out var docLength))
                        {
                            return TryAdvance(docLength - sizeof(int));
                        }
                        return false;
                    }
                case 4:
                    {
                        if (TryGetInt32(out var arrayLength))
                        {
                            return TryAdvance(arrayLength - sizeof(int));
                        }

                        return false;
                    }
                case 5:
                    {
                        if (TryGetInt32(out var binDataLength))
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
