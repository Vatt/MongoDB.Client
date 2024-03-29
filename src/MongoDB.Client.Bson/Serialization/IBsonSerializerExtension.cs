﻿using System.Diagnostics.CodeAnalysis;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Writer;

namespace MongoDB.Client.Bson.Serialization
{
    public interface IBsonSerializerExtension<T>
    {
        static abstract bool TryParseBson(ref BsonReader reader, [MaybeNullWhen(false)] out T message);
        static abstract void WriteBson(ref BsonWriter writer, in T message, out BsonType bsonType);
    }
}
