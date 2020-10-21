using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Client.Bson.Document
{
    public readonly struct BsonObjectId
    {
        public readonly int Part1 { get; }
        public readonly int Part2 { get; }
        public readonly int Part3 { get; }
        public BsonObjectId(int p1, int p2, int p3)
        {
            Part1 = p1;
            Part2 = p2;
            Part3 = p3;
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


        /// <summary>
        /// Converts a value to a hex character.
        /// </summary>
        /// <param name="value">The value (assumed to be between 0 and 15).</param>
        /// <returns>The hex character.</returns>
        public static char ToHexChar(int value)
        {
            return (char)(value + (value < 10 ? '0' : 'a' - 10));
        }
    }
}
