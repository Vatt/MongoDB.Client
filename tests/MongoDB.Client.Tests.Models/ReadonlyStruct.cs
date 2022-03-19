using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public readonly partial struct ReadonlyStruct : IEquatable<ReadonlyStruct>
    {
        public readonly int IntField;
        public readonly double DoubleField;
        public readonly string StringField;
        public ReadonlyStruct(int IntField, double DoubleField, string StringField)
        {
            this.IntField = IntField;
            this.DoubleField = DoubleField;
            this.StringField = StringField;
        }

        public bool Equals(ReadonlyStruct other)
        {
            return IntField == other.IntField && DoubleField.Equals(other.DoubleField) && StringField == other.StringField;
        }

        public override bool Equals(object? obj)
        {
            return obj is ReadonlyStruct other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IntField, DoubleField, StringField);
        }
    }
}
