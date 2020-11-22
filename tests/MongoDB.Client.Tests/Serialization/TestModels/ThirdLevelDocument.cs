using System;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Serialization.TestModels
{
    [BsonSerializable]
    public class ThirdLevelDocument : IEquatable<ThirdLevelDocument>
    {
        public string TextField { get; set; }
        public double DoubleField { get; set; }

        public bool Equals(ThirdLevelDocument other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TextField == other.TextField && DoubleField.Equals(other.DoubleField);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ThirdLevelDocument) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TextField, DoubleField);
        }
    }
}
