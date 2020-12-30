using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using System;
using System.Reflection;

namespace MongoDB.Client.Bson.Serialization
{
    public unsafe static class SerializerFnPtrProvider<T>
    {
        public static readonly delegate*<ref BsonReader, out T, bool> TryParseFnPtr;
        public static readonly delegate*<ref BsonWriter, in T, void> WriteFnPtr;
        //public static readonly delegate*<ref BsonReader, int, out T, bool> TryParseSimpleBsonType;
        public static readonly bool IsSerializable;
        public static readonly bool IsSimpleBsonType;
        static SerializerFnPtrProvider()
        {
            if (typeof(T).IsPrimitive)
            {
                TryParseFnPtr = default;
                WriteFnPtr = default;
                //TryParseSimpleBsonType = default;
                IsSerializable = false;
                IsSimpleBsonType = false;
                return;
            }
            if (typeof(T) == typeof(string) || typeof(T) == typeof(BsonDocument) ||
                typeof(T) == typeof(BsonArray) || typeof(T) == typeof(BsonObjectId) || typeof(T) == typeof(Guid) ||
                typeof(T) == typeof(DateTimeOffset))
            {
                TryParseFnPtr = default;
                WriteFnPtr = default;
                IsSerializable = false;
                IsSimpleBsonType = true;

                //if (typeof(T) == typeof(DateTimeOffset))
                //{
                //    TryParseSimpleBsonType = &TryParseDateTimeOffset;
                //}
                //if (typeof(T) == typeof(BsonObjectId))
                //{
                //    TryParseSimpleBsonType = &TryParseBsonObjectId;
                //}
                //if (typeof(T) == typeof(Guid))
                //{
                //    TryParseSimpleBsonType = &TryParseGuid;
                //}
                //if (typeof(T) == typeof(string))
                //{
                //    TryParseSimpleBsonType = &TryParseString;
                //}
                //if (typeof(T) == typeof(BsonArray))
                //{
                //    TryParseSimpleBsonType = &TryParseBsonArray;
                //}
                //if (typeof(T) == typeof(BsonDocument))
                //{
                //    TryParseSimpleBsonType = &TryParseBsonDocument;
                //}

                return;
            }

            var tryParseMethod = typeof(T).GetMethod("TryParseBson", BindingFlags.Public | BindingFlags.Static);
            var writeMethod = typeof(T).GetMethod("WriteBson", BindingFlags.Public | BindingFlags.Static);
            if (tryParseMethod == null)
            {
                TryParseFnPtr = default;
            }
            else
            {
                TryParseFnPtr = (delegate*<ref BsonReader, out T, bool>)tryParseMethod.MethodHandle.GetFunctionPointer();
            }

            if (writeMethod == null)
            {
                WriteFnPtr = default;
            }
            else
            {
                WriteFnPtr = (delegate*<ref BsonWriter, in T, void>)writeMethod.MethodHandle.GetFunctionPointer();
            }
            if ((TryParseFnPtr != default && WriteFnPtr != default) || SerializersMap.TryGetSerializer<T>(out var _))
            {
                IsSerializable = true;
                IsSimpleBsonType = false;
                //TryParseSimpleBsonType = default;
            }
        }

        private static bool TryParseString(ref BsonReader reader, int bsonType, out T genericValue)
        {
            genericValue = default(T);
            if (!reader.TryGetString(out var strvalue)) { return false; }
            genericValue = (T)(object)strvalue;
            return true;
        }
        private static bool TryParseDateTimeOffset(ref BsonReader reader, int bsonType, out T genericValue)
        {
            genericValue = default(T);
            if (!reader.TryGetDateTimeWithBsonType(bsonType, out var value)) { return false; }
            genericValue = (T)(object)value;
            return true;
        }
        private static bool TryParseBsonObjectId(ref BsonReader reader, int bsonType, out T genericValue)
        {
            genericValue = default(T);
            if (!reader.TryGetObjectId(out var value)) { return false; }
            genericValue = (T)(object)value;
            return true;
        }
        private static bool TryParseGuid(ref BsonReader reader, int bsonType, out T genericValue)
        {
            genericValue = default(T);
            if (!reader.TryGetGuidWithBsonType(bsonType, out var value)) { return false; }
            genericValue = (T)(object)value;
            return true;
        }
        private static bool TryParseBsonArray(ref BsonReader reader, int bsonType, out T genericValue)
        {
            genericValue = default(T);
            BsonDocument tempArray;
            if (!reader.TryParseDocument(out tempArray)) { return false; }
            genericValue = (T)(object)tempArray;
            return true;
        }
        private static bool TryParseBsonDocument(ref BsonReader reader, int bsonType, out T genericValue)
        {
            genericValue = default(T);
            if (!reader.TryParseDocument(out var value)) { return false; }
            genericValue = (T)(object)value;
            return true;
        }
    }
}
