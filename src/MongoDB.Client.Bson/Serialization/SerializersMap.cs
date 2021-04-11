using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MongoDB.Client.Bson.Serialization
{
    public static class SerializersMap
    {
        private static readonly Dictionary<Type, IBsonSerializer> _serializerMap = new Dictionary<Type, IBsonSerializer>
        {
            //[typeof(BsonDocument)] = new BsonDocumentSerializer(),            
        };
        //static SerializersMap()
        //{
        //    foreach (var pair in MongoDB.Client.Bson.Serialization.Generated.GlobalSerializationHelperGenerated.GetGeneratedSerializers())
        //    {
        //        _serializerMap.Add(pair.Key, pair.Value);
        //    }
        //}


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
        public static void RegisterSerializers(params KeyValuePair<Type, IBsonSerializer>[] serializers)
        {
            foreach (var pair in serializers)
            {
                _serializerMap.Add(pair.Key, pair.Value);
            }
        }
    }
}
