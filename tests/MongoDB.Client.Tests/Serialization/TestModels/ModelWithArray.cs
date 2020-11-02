using System;
using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class ModelWithArray : IEquatable<ModelWithArray>
    {
        public string Name { get; set; }
        
        [BsonIgnore] //TODO: Remove when the lists are done
        public List<int> Values { get; set; }

        public bool Equals(ModelWithArray other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Name != other.Name) return false;
            return Values.SequentialEquals(other.Values);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ModelWithArray) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Values);
        }
    }
}