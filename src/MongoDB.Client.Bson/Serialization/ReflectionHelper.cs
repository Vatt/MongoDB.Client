using System;
using System.Collections.Generic;
using System.Reflection;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
namespace MongoDB.Client.Bson.Serialization
{
    public class ReflectionHelper
    {
        internal unsafe readonly struct MethodPair
        {
            public readonly delegate*<ref BsonReader, out object, bool> TryParseFnPtr;
            public readonly delegate*<ref BsonWriter, in object, void> WriteFnPtr;
            public MethodPair(delegate*<ref BsonReader, out object, bool> tryParse, delegate*<ref BsonWriter, in object, void> write)
            {
                TryParseFnPtr = tryParse;
                WriteFnPtr = write;
            }
        }

        private static readonly Dictionary<Type, MethodPair> SerializersCache = new();

        internal static unsafe bool TryGetSerializerMethods(object target, out MethodPair methods)
        {
            methods = default;
            var type = target.GetType();
            if (SerializersCache.TryGetValue(type, out methods))
            {
                return true;
            }
            var tryParseMethod = type.GetMethod("TryParseBson", BindingFlags.Public | BindingFlags.Static);
            var writeMethod = type.GetMethod("WriteBson", BindingFlags.Public | BindingFlags.Static);
            delegate*<ref BsonReader, out object, bool> tryParseFnPtr;
            delegate*<ref BsonWriter, in object, void> writeFnPtr;
            if (tryParseMethod == null)
            {
                tryParseFnPtr = default;
            }
            else
            {
                tryParseFnPtr = (delegate*<ref BsonReader, out object, bool>)tryParseMethod.MethodHandle.GetFunctionPointer();
            }

            if (writeMethod == null)
            {
                writeFnPtr = default;
            }
            else
            {
                writeFnPtr = (delegate*<ref BsonWriter, in object, void>)writeMethod.MethodHandle.GetFunctionPointer();
            }
            if (tryParseFnPtr is not null && writeFnPtr is not null)
            {
                methods = new MethodPair(tryParseFnPtr, writeFnPtr);
                SerializersCache.Add(type, methods);
                return true;
            }
            return false;

        }
    }
}
