using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
//using MongoDB.Client.Bson.Serialization.Generated;
namespace MongoDB.Client
{
    internal static class SerializersMap
    {
        private static readonly Dictionary<Type, IBsonSerializer> _serializerMap = new Dictionary<Type, IBsonSerializer>
        {
            [typeof(BsonDocument)] = new BsonDocumentSerializer(),            
        };
        static SerializersMap()
        {
            //foreach(var pair in GlobalSerializationHelperGenerated.GetGeneratedSerializers())
            //{
            //    _serializerMap.Add(pair.Key, pair.Value);
            //}
        }
        public static bool TryGetSerializer<T>([MaybeNullWhen(false)] out IGenericBsonSerializer<T> serializer)
        {
            if (_serializerMap.TryGetValue(typeof(T), out var ser) && ser is IGenericBsonSerializer<T> typedSer)
            {
                serializer = typedSer;
                return true;
            }
            serializer = default;
            return false;
        }
    }
}
