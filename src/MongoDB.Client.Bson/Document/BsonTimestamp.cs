using System;
using System.Buffers.Binary;

namespace MongoDB.Client.Bson.Document
{
    public readonly struct BsonTimestamp : IEquatable<BsonTimestamp>
    {
        private readonly long _value;

        public BsonTimestamp(int timestamp, int increment)
        {
            _value = (long)(((ulong)(uint)timestamp << 32) | (ulong)(uint)increment);
        }

        public BsonTimestamp(long timestamp)
        {
            _value = timestamp;
        }

        public int Timestamp => (int)(_value >> 32);
        public int Increment => (int)_value;

        public bool TryWriteBytes(Span<byte> destination)
        {
            if (destination.Length >= sizeof(long))
            {
                BinaryPrimitives.WriteInt64LittleEndian(destination, _value);
                return true;
            }

            return false;
        }
        public static bool operator ==(BsonTimestamp left, BsonTimestamp rigth)
        {
            return left.Equals(rigth);
        }

        public static bool operator !=(BsonTimestamp left, BsonTimestamp rigth)
        {
            return left.Equals(rigth) == false;
        }
        public override string ToString()
        {
            return $"Timestamp({Timestamp}, {Increment})";
        }

        public override bool Equals(object? obj)
        {
            return obj is BsonTimestamp timestamp && Equals(timestamp);
        }

        public bool Equals(BsonTimestamp other)
        {
            return _value == other._value &&
                   Timestamp == other.Timestamp &&
                   Increment == other.Increment;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_value, Timestamp, Increment);
        }
    }
}
