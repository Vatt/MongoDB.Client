using System;
using System.Buffers.Binary;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using MongoDB.Client.Bson.Utils;

namespace MongoDB.Client.Bson.Document
{
    public readonly struct BsonObjectId : IEquatable<BsonObjectId>
    {
        private static int _increment;
        private static readonly long _random;

        static BsonObjectId()
        {
            using (var rand = RandomNumberGenerator.Create())
            {
                Span<byte> buffer = stackalloc byte[8];
                rand.GetBytes(buffer);
                _increment = MemoryMarshal.Read<int>(buffer);
                rand.GetNonZeroBytes(buffer);
                _random = MemoryMarshal.Read<long>(buffer);
            }
        }

        public readonly int Part1 { get; }
        public readonly int Part2 { get; }
        public readonly int Part3 { get; }

        public BsonObjectId(int p1, int p2, int p3)
        {
            Part1 = p1;
            Part2 = p2;
            Part3 = p3;
        }

        public BsonObjectId(ReadOnlySpan<byte> span)
        {
            if (span.Length < 12)
            {
                ThrowHelper.ObjectIdParseException();
            }

            Part1 = BinaryPrimitives.ReadInt32BigEndian(span);
            Part2 = BinaryPrimitives.ReadInt32BigEndian(span.Slice(4));
            Part3 = BinaryPrimitives.ReadInt32BigEndian(span.Slice(8));
        }

        public BsonObjectId(ReadOnlySpan<char> value)
        {
            if (value.Length < 24)
            {
                ThrowHelper.ObjectIdParseException();
            }

            if (int.TryParse(value.Slice(0, 8), NumberStyles.AllowHexSpecifier, null, out int part1) == false)
            {
                ThrowHelper.ObjectIdParseException();
            }

            if (int.TryParse(value.Slice(8, 8), NumberStyles.AllowHexSpecifier, null, out int part2) == false)
            {
                ThrowHelper.ObjectIdParseException();
            }

            if (int.TryParse(value.Slice(16, 8), NumberStyles.AllowHexSpecifier, null, out int part3) == false)
            {
                ThrowHelper.ObjectIdParseException();
            }

            Part1 = part1;
            Part2 = part2;
            Part3 = part3;
        }

        public static BsonObjectId NewObjectId()
        {
            var timestamp = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var increment = Interlocked.Increment(ref _increment);
            var a = timestamp;
            var b = (int)(_random >> 8); // first 4 bytes of random
            var c = (int)(_random << 24) | increment; // 5th byte of random and 3 byte increment
            return new BsonObjectId(a, b, c);
        }

        public override string ToString()
        {
            Span<char> c = stackalloc char[24];
            c[0] = ToHexChar((Part1 >> 28) & 0x0f);
            c[1] = ToHexChar((Part1 >> 24) & 0x0f);
            c[2] = ToHexChar((Part1 >> 20) & 0x0f);
            c[3] = ToHexChar((Part1 >> 16) & 0x0f);
            c[4] = ToHexChar((Part1 >> 12) & 0x0f);
            c[5] = ToHexChar((Part1 >> 8) & 0x0f);
            c[6] = ToHexChar((Part1 >> 4) & 0x0f);
            c[7] = ToHexChar(Part1 & 0x0f);
            c[8] = ToHexChar((Part2 >> 28) & 0x0f);
            c[9] = ToHexChar((Part2 >> 24) & 0x0f);
            c[10] = ToHexChar((Part2 >> 20) & 0x0f);
            c[11] = ToHexChar((Part2 >> 16) & 0x0f);
            c[12] = ToHexChar((Part2 >> 12) & 0x0f);
            c[13] = ToHexChar((Part2 >> 8) & 0x0f);
            c[14] = ToHexChar((Part2 >> 4) & 0x0f);
            c[15] = ToHexChar(Part2 & 0x0f);
            c[16] = ToHexChar((Part3 >> 28) & 0x0f);
            c[17] = ToHexChar((Part3 >> 24) & 0x0f);
            c[18] = ToHexChar((Part3 >> 20) & 0x0f);
            c[19] = ToHexChar((Part3 >> 16) & 0x0f);
            c[20] = ToHexChar((Part3 >> 12) & 0x0f);
            c[21] = ToHexChar((Part3 >> 8) & 0x0f);
            c[22] = ToHexChar((Part3 >> 4) & 0x0f);
            c[23] = ToHexChar(Part3 & 0x0f);
            return new string(c);
        }

        public bool TryWriteBytes(Span<byte> destination)
        {
            if (destination.Length >= 12)
            {
                BinaryPrimitives.WriteInt32BigEndian(destination, Part1);
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(4), Part2);
                BinaryPrimitives.WriteInt32BigEndian(destination.Slice(8), Part3);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Converts a value to a hex character.
        /// </summary>
        /// <param name="value">The value (assumed to be between 0 and 15).</param>
        /// <returns>The hex character.</returns>
        private static char ToHexChar(int value)
        {
            return (char) (value + (value < 10 ? '0' : 'a' - 10));
        }

        public bool Equals(BsonObjectId other)
        {
            return Part1 == other.Part1 && Part2 == other.Part2 && Part3 == other.Part3;
        }
    }
}