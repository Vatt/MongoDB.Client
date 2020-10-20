using System;
using System.Collections.Generic;


namespace MongoDB.Client.Bson.Serialization
{
    public static class GlobalSerialization
    {
        public static readonly Dictionary<Type, IBsonSerializable> Serializators = new Dictionary<Type, IBsonSerializable>();
    }
}
