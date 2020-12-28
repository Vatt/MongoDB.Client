using System;
using System.Collections.Generic;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public partial class SecondLevelDocument : IEquatable<SecondLevelDocument>
    {
        public string TextField { get; set; }
        public int IntField { get; set; }
        public List<ThirdLevelDocument> InnerDocuments { get; set; }

        public bool Equals(SecondLevelDocument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TextField == other.TextField && IntField == other.IntField && InnerDocuments.SequentialEquals(other.InnerDocuments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SecondLevelDocument) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TextField, IntField, InnerDocuments);
        }
    }
}
