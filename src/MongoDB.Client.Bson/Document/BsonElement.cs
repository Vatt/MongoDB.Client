using System.Diagnostics;

namespace MongoDB.Client.Bson.Document
{
    [DebuggerDisplay("{Value}", Name = "{Name}")]
    public readonly struct BsonElement : IEquatable<BsonElement>
    {
        public readonly BsonDocument Parent { get; }
        public readonly BsonType Type { get; }
        public readonly string Name { get; }

        public object? Value { get; }

        private BsonElement(BsonDocument parent, BsonType type, string name, object? value)
        {
            Parent = parent;
            Type = type;
            Name = name;
            Value = value;
        }

        public static BsonElement Create(BsonDocument parent, string name, int value)
        {
            return new BsonElement(parent, BsonType.Int32, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, decimal value)
        {
            return new BsonElement(parent, BsonType.Decimal, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, long value)
        {
            return new BsonElement(parent, BsonType.Int64, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, string? value)
        {
            if (value is not null)
            {
                return new BsonElement(parent, BsonType.String, name, value);
            }
            return new BsonElement(parent, BsonType.Null, name, null);
        }

        public static BsonElement Create(BsonDocument parent, string name, double value)
        {
            return new BsonElement(parent, BsonType.Double, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, BsonDocument? value)
        {
            if (value is not null)
            {
                return new BsonElement(parent, BsonType.Document, name, value);
            }
            return new BsonElement(parent, BsonType.Null, name, null);
        }

        public static BsonElement Create(BsonDocument parent, string name, BsonObjectId value)
        {
            return new BsonElement(parent, BsonType.ObjectId, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, BsonTimestamp value)
        {
            return new BsonElement(parent, BsonType.Timestamp, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, BsonBinaryData value)
        {
            return new BsonElement(parent, BsonType.BinaryData, name, value);
        }


        public static BsonElement Create(BsonDocument parent, string name, DateTimeOffset value)
        {
            return new BsonElement(parent, BsonType.UtcDateTime, name, value);
        }

        public static BsonElement Create(BsonDocument parent, string name, bool value)
        {
            return new BsonElement(parent, BsonType.Boolean, name, value);
        }


        public static BsonElement Create(BsonDocument parent, string name)
        {
            return new BsonElement(parent, BsonType.Null, name, null);
        }


        public static BsonElement CreateArray(BsonDocument parent, string name, BsonDocument? root)
        {
            if (root is not null)
            {
                return new BsonElement(parent, BsonType.Array, name, root);
            }
            return new BsonElement(parent, BsonType.Null, name, null);
        }

        public BsonDocument? AsBsonDocument => (BsonDocument?)Value;
        public string? AsString => (string?)Value;

        public override string ToString()
        {
            return "\"" + Name + "\" : " + ValueToString(Value) + "";
        }

        private static string? ValueToString(object? value)
        {
            if (value is null)
            {
                return "null";
            }

            switch (value)
            {
                case string val:
                    return "\"" + val + "\"";
                case bool val:
                    return val.ToString().ToLowerInvariant();
                case long val:
                    return $"NumberLong({val})";
                default:
                    return value.ToString();
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is BsonElement element && Equals(element);
        }

        public bool Equals(BsonElement element)
        {
            return Type == element.Type && Name == element.Name && EqualityComparer<object?>.Default.Equals(Value, element.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Name, Value);
        }
        public bool IsEmpty => Name is null;
    }
}
