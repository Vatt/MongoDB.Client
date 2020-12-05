using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;
using System.Reflection;

namespace MongoDB.Client.Bson.Serialization
{
    public unsafe static class SerializerFnPtrProvider<T>
    {
        public static readonly delegate*<ref BsonReader, out T, bool> TryParseFnPtr;
        public static readonly delegate*<ref BsonWriter, in T, void> WriteFnPtr;
        static SerializerFnPtrProvider()
        {
            var tryParseMethod = typeof(T).GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static);
            var writeMethod = typeof(T).GetMethod("Write", BindingFlags.Public | BindingFlags.Static);
            if (tryParseMethod == null)
            {
                TryParseFnPtr = default;
            }
            else
            {
                TryParseFnPtr = (delegate*<ref BsonReader, out T, bool>)tryParseMethod.MethodHandle.GetFunctionPointer();
            }
            
            if(writeMethod == null)
            {
                WriteFnPtr = default;
            }
            else
            {
                WriteFnPtr = (delegate*<ref BsonWriter, in T, void>)writeMethod.MethodHandle.GetFunctionPointer();
            }
        }
    }
}
