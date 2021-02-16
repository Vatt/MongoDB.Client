using MongoDB.Client.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class FirstLevelDocument : IEquatable<FirstLevelDocument>
    {
        public string TextField { get; set; }

        public int IntField { get; set; }
        public List<SecondLevelDocument> InnerDocuments { get; set; }

        public bool Equals(FirstLevelDocument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TextField == other.TextField && IntField == other.IntField && InnerDocuments.SequenceEqual(other.InnerDocuments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FirstLevelDocument)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TextField, IntField, InnerDocuments);
        }
    }
}
