using System;
using System.Linq;
using MongoDB.Client.Bson.Serialization;

namespace MongoDB.Client.Tests.Models
{
    public class CustomModel : IEquatable<CustomModel>, IBsonSerializer<CustomModel>
    {
        public int A, B, C;
        public static CustomModel Create()
        {
            return new CustomModel { A = 42, B = 42, C = 42 };
        }

        public bool Equals(CustomModel? other)
        {
            return other is not null && A == other.A && B == other.B && C == other.C;
        }


        private static ReadOnlySpan<byte> CustomModelA => new byte[1] { 65 };
        private static ReadOnlySpan<byte> CustomModelB => new byte[1] { 66 };
        private static ReadOnlySpan<byte> CustomModelC => new byte[1] { 67 };
        public static bool TryParseBson(ref Bson.Reader.BsonReader reader, out CustomModel message)
        {
            message = default!;
            int Int32A = default;
            int Int32B = default;
            int Int32C = default;
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

                if (bsonName.SequenceEqual(CustomModelA))
                {
                    if (!reader.TryGetInt32(out Int32A))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(CustomModelB))
                {
                    if (!reader.TryGetInt32(out Int32B))
                    {
                        return false;
                    }

                    continue;
                }

                if (bsonName.SequenceEqual(CustomModelC))
                {
                    if (!reader.TryGetInt32(out Int32C))
                    {
                        return false;
                    }

                    continue;
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
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Tests.Models.CustomModel), endMarker);
            }

            message = new MongoDB.Client.Tests.Models.CustomModel();
            message.A = Int32A;
            message.B = Int32B;
            message.C = Int32C;
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Tests.Models.CustomModel message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            writer.Write_Type_Name_Value(CustomModelA, message.A);
            writer.Write_Type_Name_Value(CustomModelB, message.B);
            writer.Write_Type_Name_Value(CustomModelC, message.C);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }


    }
}
