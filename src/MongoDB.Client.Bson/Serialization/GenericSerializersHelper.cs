using MongoDB.Client.Bson.Reader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Serialization
{
    internal unsafe class FnPtrWrapper
    {
        private delegate*<ref MongoDBBsonReader, void*, bool> FnPtr;

        public FnPtrWrapper(delegate*<ref MongoDBBsonReader, void*, bool> fn)
        {
            FnPtr = fn;
        }
    }
    internal unsafe class SerializersHelper
    {
        internal unsafe Dictionary<Type, FnPtrWrapper> TypeReaders = new Dictionary<Type, FnPtrWrapper> ()
        {
            [typeof(int)] = new FnPtrWrapper(&TryGetInt32),
            [typeof(long)] = new FnPtrWrapper(&TryGetInt64),
            [typeof(double)] = new FnPtrWrapper(&TryGetDouble)
        };
        public static unsafe bool TryGetInt32(ref MongoDBBsonReader reader, void* destination)
        {
            return reader.TryGetInt32(out *(int*)destination);
        }
        public static unsafe bool TryGetInt64(ref MongoDBBsonReader reader, void* destination)
        {
            return reader.TryGetInt64(out *(long*)destination);
        }
        public static unsafe bool TryGetDouble(ref MongoDBBsonReader reader, void* destination)
        {
            return reader.TryGetDouble(out *(double*)destination);
        }
    }
}
