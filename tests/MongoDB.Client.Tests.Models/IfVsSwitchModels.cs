using System;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class IfShortNamesModel : IEquatable<IfShortNamesModel>
    {
        public int AAA11;
        public int AAA12;
        public int AAA13;
        public int AAA14;
        public int AAA15;
        public int AAA16;
        public int BBB11;
        public int BBB12;
        public int BBB13;
        public int BBB14;
        public int BBB15;
        public int BBB16;
        /*public int A;
        public int BB;
        public int CCC;
        public int DDDD;
        public int EEEEE;
        public int FFFFFF;
        public int GGGGGGG;
        public int HHHHHHHH;
        public int IIIIIIIII;
        public int JJJJJJJJJJ;
        public int KKKKKKKKKKK;
        public int LLLLLLLLLLLL;*/

        public static IfShortNamesModel Create()
        {
            return new IfShortNamesModel
            {
                AAA11 = 42,
                AAA12 = 42,
                AAA13 = 42,
                AAA14 = 42,
                AAA15 = 42,
                AAA16 = 42,
                BBB11 = 42,
                BBB12 = 42,
                BBB13 = 42,
                BBB14 = 42,
                BBB15 = 42,
                BBB16 = 42,
                /*A = 42,
                BB = 42,
                CCC = 42,
                DDDD = 42,
                EEEEE = 42,
                FFFFFF = 42,
                GGGGGGG = 42,
                HHHHHHHH = 42,
                IIIIIIIII = 42,
                JJJJJJJJJJ = 42,
                KKKKKKKKKKK = 42,
                LLLLLLLLLLLL = 42,*/
            };
        }

        public bool Equals(IfShortNamesModel? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return AAA11 == other.AAA11 && AAA12 == other.AAA12 && AAA13 == other.AAA13 && AAA14 == other.AAA14 && AAA15 == other.AAA15 && AAA16 == other.AAA16 && BBB11 == other.BBB11 && BBB12 == other.BBB12 && BBB13 == other.BBB13 && BBB14 == other.BBB14 && BBB15 == other.BBB15 && BBB16 == other.BBB16;
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
            hashCode.Add(AAA11);
            hashCode.Add(AAA12);
            hashCode.Add(AAA13);
            hashCode.Add(AAA14);
            hashCode.Add(AAA15);
            hashCode.Add(AAA16);
            hashCode.Add(BBB11);
            hashCode.Add(BBB12);
            hashCode.Add(BBB13);
            hashCode.Add(BBB14);
            hashCode.Add(BBB15);
            hashCode.Add(BBB16);
            return hashCode.ToHashCode();
        }
    }
    [BsonSerializable(GeneratorMode.ContextTree)]
    public partial class SwitchShortNamesModel : IEquatable<SwitchShortNamesModel>
    {
        public int AAA11;
        public int AAA12;
        public int AAA13;
        public int AAA14;
        public int AAA15;
        public int AAA16;
        public int BBB11;
        public int BBB12;
        public int BBB13;
        public int BBB14;
        public int BBB15;

        public int BBB16;
        /*public int A;
        public int BB;
        public int CCC;
        public int DDDD;
        public int EEEEE;
        public int FFFFFF;
        public int GGGGGGG;
        public int HHHHHHHH;
        public int IIIIIIIII;
        public int JJJJJJJJJJ;
        public int KKKKKKKKKKK;
        public int LLLLLLLLLLLL;*/

        public static SwitchShortNamesModel Create()
        {
            return new SwitchShortNamesModel
            {
                AAA11 = 42,
                AAA12 = 42,
                AAA13 = 42,
                AAA14 = 42,
                AAA15 = 42,
                AAA16 = 42,
                BBB11 = 42,
                BBB12 = 42,
                BBB13 = 42,
                BBB14 = 42,
                BBB15 = 42,
                BBB16 = 42,
                /*A = 42,
                BB = 42,
                CCC = 42,
                DDDD = 42,
                EEEEE = 42,
                FFFFFF = 42,
                GGGGGGG = 42,
                HHHHHHHH = 42,
                IIIIIIIII = 42,
                JJJJJJJJJJ = 42,
                KKKKKKKKKKK = 42,
                LLLLLLLLLLLL = 42,*/
            };
        }

        public bool Equals(SwitchShortNamesModel? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return AAA11 == other.AAA11 && AAA12 == other.AAA12 && AAA13 == other.AAA13 && AAA14 == other.AAA14 && AAA15 == other.AAA15 && AAA16 == other.AAA16 && BBB11 == other.BBB11 && BBB12 == other.BBB12 && BBB13 == other.BBB13 && BBB14 == other.BBB14 && BBB15 == other.BBB15 && BBB16 == other.BBB16;
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
            hashCode.Add(AAA11);
            hashCode.Add(AAA12);
            hashCode.Add(AAA13);
            hashCode.Add(AAA14);
            hashCode.Add(AAA15);
            hashCode.Add(AAA16);
            hashCode.Add(BBB11);
            hashCode.Add(BBB12);
            hashCode.Add(BBB13);
            hashCode.Add(BBB14);
            hashCode.Add(BBB15);
            hashCode.Add(BBB16);
            return hashCode.ToHashCode();
        }
    }
    [BsonSerializable(GeneratorMode.ContextTree)]
    public partial class SwitchGroupNamesModel
    {
        public int AAA11;
        public int AAA12;
        public int AAA13;
        public int AAA14;
        public int AAA15;
        public int BBB11;
        public int BBB12;
        public int BBB13;
        public int BBB14;
        public int BBB15;
        public static SwitchGroupNamesModel Create()
        {
            return new SwitchGroupNamesModel
            {
                AAA11 = 42,
                AAA12 = 42,
                AAA13 = 42,
                AAA14 = 42,
                AAA15 = 42,
                BBB11 = 42,
                BBB12 = 42,
                BBB13 = 42,
                BBB14 = 42,
                BBB15 = 42,
            };
        }
    }
    [BsonSerializable(GeneratorMode.ContextTree)]
    public partial class SwitchNonGroupNamesModel
    {
        public int AAA10;
        public int AAA11;
        public int AAA12;
        public int AAA13;
        public int AAA14;
        public int AAA15;
        public int AAA16;
        public int AAA17;
        public int AAA18;
        public int AAA19;
        public static SwitchNonGroupNamesModel Create()
        {
            return new SwitchNonGroupNamesModel
            {
                AAA10 = 42,
                AAA11 = 42,
                AAA12 = 42,
                AAA13 = 42,
                AAA14 = 42,
                AAA15 = 42,
                AAA16 = 42,
                AAA17 = 42,
                AAA18 = 42,
                AAA19 = 42,
            };
        }
    }
    [BsonSerializable(GeneratorMode.ContextTree)]
    public partial class ContextTreeGroupNamesModel
    {
        public int AAA11;
        public int AAA12;
        public int AAA13;
        public int AAA14;
        public int AAA15;
        public int BBB11;
        public int BBB12;
        public int BBB13;
        public int BBB14;
        public int BBB15;
        public static ContextTreeGroupNamesModel Create()
        {
            return new ContextTreeGroupNamesModel
            {
                AAA11 = 42,
                AAA12 = 42,
                AAA13 = 42,
                AAA14 = 42,
                AAA15 = 42,
                BBB11 = 42,
                BBB12 = 42,
                BBB13 = 42,
                BBB14 = 42,
                BBB15 = 42,
            };
        }
    }

}