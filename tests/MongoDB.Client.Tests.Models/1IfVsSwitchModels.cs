using System;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class IfShortNamesModel : IEquatable<IfShortNamesModel>
    {
        public int A;
        public double BB;
        public string CCC;
        public DateTimeOffset DDDD;
        public long EEEEE;
        public BsonDocument FFFFFF;
        public int GGGGGGG;
        public double HHHHHHHH;
        public string IIIIIIIII;
        public DateTimeOffset JJJJJJJJJJ;
        public long KKKKKKKKKKK;
        public BsonDocument LLLLLLLLLLLL;

        public static IfShortNamesModel Create()
        {
            return new IfShortNamesModel
            {
                A = 42,
                BB = 42,
                CCC = "42",
                DDDD = new DateTimeOffset(2021, 03, 06, 21, 48, 42, TimeSpan.Zero),
                EEEEE = 42,
                FFFFFF = new BsonDocument("42", "42"),
                GGGGGGG = 42,
                HHHHHHHH = 42,
                IIIIIIIII = "42",
                JJJJJJJJJJ = new DateTimeOffset(2021, 03, 06, 21, 48, 42, TimeSpan.Zero),
                KKKKKKKKKKK = 42,
                LLLLLLLLLLLL = new BsonDocument("42", "42")
            };
        }

        public bool Equals(IfShortNamesModel? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return A == other.A && BB.Equals(other.BB) && CCC == other.CCC && DDDD.Equals(other.DDDD) && 
                   EEEEE == other.EEEEE && FFFFFF.Equals(other.FFFFFF) && GGGGGGG == other.GGGGGGG && 
                   HHHHHHHH.Equals(other.HHHHHHHH) && IIIIIIIII == other.IIIIIIIII && 
                   JJJJJJJJJJ.Equals(other.JJJJJJJJJJ) && KKKKKKKKKKK == other.KKKKKKKKKKK && 
                   LLLLLLLLLLLL.Equals(other.LLLLLLLLLLLL);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IfShortNamesModel) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(A);
            hashCode.Add(BB);
            hashCode.Add(CCC);
            hashCode.Add(DDDD);
            hashCode.Add(EEEEE);
            hashCode.Add(FFFFFF);
            hashCode.Add(GGGGGGG);
            hashCode.Add(HHHHHHHH);
            hashCode.Add(IIIIIIIII);
            hashCode.Add(JJJJJJJJJJ);
            hashCode.Add(KKKKKKKKKKK);
            hashCode.Add(LLLLLLLLLLLL);
            return hashCode.ToHashCode();
        }
    }

    [BsonSerializable(GeneratorMode.SwitchOperations)]
    public partial class SwitchShortNamesModel : IEquatable<SwitchShortNamesModel>
    {
        public int A;
        public double BB;
        public string CCC;
        public DateTimeOffset DDDD;
        public long EEEEE;
        public BsonDocument FFFFFF;
        public int GGGGGGG;
        public double HHHHHHHH;
        public string IIIIIIIII;
        public DateTimeOffset JJJJJJJJJJ;
        public long KKKKKKKKKKK;
        public BsonDocument LLLLLLLLLLLL;

        public static SwitchShortNamesModel Create()
        {
            return new SwitchShortNamesModel
            {
                A = 42,
                BB = 42,
                CCC = "42",
                DDDD = new DateTimeOffset(2021, 03, 06, 21, 48, 42, TimeSpan.Zero),
                EEEEE = 42,
                FFFFFF = new BsonDocument("42", "42"),
                GGGGGGG = 42,
                HHHHHHHH = 42,
                IIIIIIIII = "42",
                JJJJJJJJJJ = new DateTimeOffset(2021, 03, 06, 21, 48, 42, TimeSpan.Zero),
                KKKKKKKKKKK = 42,
                LLLLLLLLLLLL = new BsonDocument("42", "42")
            };
        }

        public bool Equals(SwitchShortNamesModel? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return A == other.A && BB.Equals(other.BB) && CCC == other.CCC && DDDD.Equals(other.DDDD) && 
                   EEEEE == other.EEEEE && FFFFFF.Equals(other.FFFFFF) && GGGGGGG == other.GGGGGGG && 
                   HHHHHHHH.Equals(other.HHHHHHHH) && IIIIIIIII == other.IIIIIIIII && 
                   JJJJJJJJJJ.Equals(other.JJJJJJJJJJ) && KKKKKKKKKKK == other.KKKKKKKKKKK && 
                   LLLLLLLLLLLL.Equals(other.LLLLLLLLLLLL);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SwitchShortNamesModel) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(A);
            hashCode.Add(BB);
            hashCode.Add(CCC);
            hashCode.Add(DDDD);
            hashCode.Add(EEEEE);
            hashCode.Add(FFFFFF);
            hashCode.Add(GGGGGGG);
            hashCode.Add(HHHHHHHH);
            hashCode.Add(IIIIIIIII);
            hashCode.Add(JJJJJJJJJJ);
            hashCode.Add(KKKKKKKKKKK);
            hashCode.Add(LLLLLLLLLLLL);
            return hashCode.ToHashCode();
        }
    }
}