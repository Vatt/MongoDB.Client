using System;
using System.Buffers.Binary;

namespace MongoDB.Client.Bson.Document
{
    public readonly struct BsonTimestamp
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
                BinaryPrimitives.WriteInt64BigEndian(destination, _value);
                return true;
            }

            return false;
        }
    }
}