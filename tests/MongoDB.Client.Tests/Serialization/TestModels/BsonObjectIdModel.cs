using System;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class BsonObjectIdModel
    {
        [BsonId]
        public BsonObjectId Id { get; set; }

        public int SomeInt { get; set; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return obj is BsonObjectIdModel other && other.Id.Equals(other.Id) && SomeInt == other.SomeInt;
        }

        public override int GetHashCode() => HashCode.Combine(Id, SomeInt);
    }
}
