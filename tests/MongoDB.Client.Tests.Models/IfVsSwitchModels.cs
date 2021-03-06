using System;
using System.Buffers.Binary;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial class IfShortNamesModel
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
    }

    public class SwitchShortNamesModel
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
        private static ReadOnlySpan<byte> IfShortNamesModelA => new byte[1] {65};
        private static ReadOnlySpan<byte> IfShortNamesModelBB => new byte[2] {66, 66};
        private static ReadOnlySpan<byte> IfShortNamesModelCCC => new byte[3] {67, 67, 67};
        private static ReadOnlySpan<byte> IfShortNamesModelDDDD => new byte[4] {68, 68, 68, 68};
        private static ReadOnlySpan<byte> IfShortNamesModelEEEEE => new byte[5] {69, 69, 69, 69, 69};
        private static ReadOnlySpan<byte> IfShortNamesModelFFFFFF => new byte[6] {70, 70, 70, 70, 70, 70};
        private static ReadOnlySpan<byte> IfShortNamesModelGGGGGGG => new byte[7] {71, 71, 71, 71, 71, 71, 71};
        private static ReadOnlySpan<byte> IfShortNamesModelHHHHHHHH => new byte[8] {72, 72, 72, 72, 72, 72, 72, 72};

        private static ReadOnlySpan<byte> IfShortNamesModelIIIIIIIII => new byte[9] {73, 73, 73, 73, 73, 73, 73, 73, 73};

        private static ReadOnlySpan<byte> IfShortNamesModelJJJJJJJJJJ => new byte[10] {74, 74, 74, 74, 74, 74, 74, 74, 74, 74};

        private static ReadOnlySpan<byte> IfShortNamesModelKKKKKKKKKKK => new byte[11] {75, 75, 75, 75, 75, 75, 75, 75, 75, 75, 75};

        private static ReadOnlySpan<byte> IfShortNamesModelLLLLLLLLLLLL => new byte[12] {76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76, 76};

        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoDB.Client.Tests.Models.SwitchShortNamesModel message)
        {
            message = default;
            int Int32A = default;
            double DoubleBB = default;
            string StringCCC = default;
            System.DateTimeOffset DateTimeOffsetDDDD = default;
            long Int64EEEEE = default;
            MongoDB.Client.Bson.Document.BsonDocument BsonDocumentFFFFFF = default;
            int Int32GGGGGGG = default;
            double DoubleHHHHHHHH = default;
            string StringIIIIIIIII = default;
            System.DateTimeOffset DateTimeOffsetJJJJJJJJJJ = default;
            long Int64KKKKKKKKKKK = default;
            MongoDB.Client.Bson.Document.BsonDocument BsonDocumentLLLLLLLLLLLL = default;
            if (!reader.TryGetInt32(out int docLength))
            {
                return false;
            }

            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                if (!reader.TryGetByte(out var bsonType))
                {
                    return false;
                }

                if (!reader.TryGetCStringAsSpan(out var bsonName))
                {
                    return false;
                }

                if (bsonType == 10)
                {
                    continue;
                }

                switch (bsonName.Length)
                {
                    case 1:
                    {
                        if (!reader.TryGetInt32(out Int32A))
                        {
                            return false;
                        }
                        continue;
                    }
                    case 2:
                    {
                        if (!reader.TryGetDouble(out DoubleBB))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 3:
                    {
                        if (!reader.TryGetString(out StringCCC))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 4:
                    {
                        if (!reader.TryGetDateTimeWithBsonType(bsonType, out DateTimeOffsetDDDD))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 5:
                    {
                        if (!reader.TryGetInt64(out Int64EEEEE))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 6:
                    {
                        if (!reader.TryParseDocument(out BsonDocumentFFFFFF))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 7:
                    {
                        if (!reader.TryGetInt32(out Int32GGGGGGG))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 8:
                    {
                        if (!reader.TryGetDouble(out DoubleHHHHHHHH))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 9:
                    {
                        if (!reader.TryGetString(out StringIIIIIIIII))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 10:
                    {
                        if (!reader.TryGetDateTimeWithBsonType(bsonType, out DateTimeOffsetJJJJJJJJJJ))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 11:
                    {
                        if (!reader.TryGetInt64(out Int64KKKKKKKKKKK))
                        {
                            return false;
                        }

                        continue;
                    }
                    case 12:
                    {
                        if (!reader.TryParseDocument(out BsonDocumentLLLLLLLLLLLL))
                        {
                            return false;
                        }

                        continue;
                    }
                }

                if (!reader.TrySkip(bsonType))
                {
                    return false;
                }
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(
                    nameof(MongoDB.Client.Tests.Models.IfShortNamesModel), endMarker);
            }

            message = new MongoDB.Client.Tests.Models.SwitchShortNamesModel();
            message.A = Int32A;
            message.BB = DoubleBB;
            message.CCC = StringCCC;
            message.DDDD = DateTimeOffsetDDDD;
            message.EEEEE = Int64EEEEE;
            message.FFFFFF = BsonDocumentFFFFFF;
            message.GGGGGGG = Int32GGGGGGG;
            message.HHHHHHHH = DoubleHHHHHHHH;
            message.IIIIIIIII = StringIIIIIIIII;
            message.JJJJJJJJJJ = DateTimeOffsetJJJJJJJJJJ;
            message.KKKKKKKKKKK = Int64KKKKKKKKKKK;
            message.LLLLLLLLLLLL = BsonDocumentLLLLLLLLLLLL;
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Tests.Models.SwitchShortNamesModel message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            writer.Write_Type_Name_Value(IfShortNamesModelA, message.A);
            writer.Write_Type_Name_Value(IfShortNamesModelBB, message.BB);
            if (message.CCC == null)
            {
                writer.WriteBsonNull(IfShortNamesModelCCC);
            }
            else
            {
                writer.Write_Type_Name_Value(IfShortNamesModelCCC, message.CCC);
            }

            writer.Write_Type_Name_Value(IfShortNamesModelDDDD, message.DDDD);
            writer.Write_Type_Name_Value(IfShortNamesModelEEEEE, message.EEEEE);
            if (message.FFFFFF == null)
            {
                writer.WriteBsonNull(IfShortNamesModelFFFFFF);
            }
            else
            {
                writer.Write_Type_Name_Value(IfShortNamesModelFFFFFF, message.FFFFFF);
            }

            writer.Write_Type_Name_Value(IfShortNamesModelGGGGGGG, message.GGGGGGG);
            writer.Write_Type_Name_Value(IfShortNamesModelHHHHHHHH, message.HHHHHHHH);
            if (message.IIIIIIIII == null)
            {
                writer.WriteBsonNull(IfShortNamesModelIIIIIIIII);
            }
            else
            {
                writer.Write_Type_Name_Value(IfShortNamesModelIIIIIIIII, message.IIIIIIIII);
            }

            writer.Write_Type_Name_Value(IfShortNamesModelJJJJJJJJJJ, message.JJJJJJJJJJ);
            writer.Write_Type_Name_Value(IfShortNamesModelKKKKKKKKKKK, message.KKKKKKKKKKK);
            if (message.LLLLLLLLLLLL == null)
            {
                writer.WriteBsonNull(IfShortNamesModelLLLLLLLLLLLL);
            }
            else
            {
                writer.Write_Type_Name_Value(IfShortNamesModelLLLLLLLLLLLL, message.LLLLLLLLLLLL);
            }

            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            reserved.Write(sizeSpan);
            writer.Commit();
        }
    }
}