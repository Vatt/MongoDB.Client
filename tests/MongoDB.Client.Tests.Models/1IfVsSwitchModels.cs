using System;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class IfShortNamesModel
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
    }
    [BsonSerializable(GeneratorMode.ContextTree)]
    public partial class SwitchShortNamesModel
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