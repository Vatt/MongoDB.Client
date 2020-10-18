using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
