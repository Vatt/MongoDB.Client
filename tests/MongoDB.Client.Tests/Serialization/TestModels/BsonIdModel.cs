﻿using MongoDB.Client.Bson.Serialization.Attributes;
using System;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class BsonIdModel
    {
        [BsonIdAttribute]
        public Guid Id;
        public int SomeInt;
        [BsonConstructor]
        public BsonIdModel(Guid Id, int SomeInt)
        {
            this.Id = Id;
            this.SomeInt = SomeInt;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return obj is BsonIdModel other && other.Id.Equals(other.Id) && SomeInt == other.SomeInt;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, SomeInt);
        }
    }
}
