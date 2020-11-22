﻿using System;

namespace MongoDB.Client.Bson.Serialization.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class BsonSerializableAttribute : Attribute
    {
        public BsonSerializableAttribute()
        {

        }
    }
}
