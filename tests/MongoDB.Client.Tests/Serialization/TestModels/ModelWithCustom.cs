using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;
using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MongoDB.Client.Tests.Serialization.TestModels
{

    public record CustomModel(int A, int B, int C);
    public record CustomModel2(int A, int B, int C)
    {
        private static ReadOnlySpan<byte> CustomModelA => new byte[1] { 65 };
        private static ReadOnlySpan<byte> CustomModelB => new byte[1] { 66 };
        private static ReadOnlySpan<byte> CustomModelC => new byte[1] { 67 };
        public static bool TryParseBson(ref BsonReader reader, [MaybeNullWhen(false)] out CustomModel2 message)
        {
            message = default;
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
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Tests.Serialization.TestModels.CustomModel), endMarker);
            }

            message = new MongoDB.Client.Tests.Serialization.TestModels.CustomModel2(A: Int32A, B: Int32B, C: Int32C);
            return true;
        }

        public static void WriteBson(ref BsonWriter writer, in CustomModel2 message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            writer.Write_Type_Name_Value(CustomModelA, message.A);
            writer.Write_Type_Name_Value(CustomModelB, message.B);
            writer.Write_Type_Name_Value(CustomModelC, message.C);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            reserved.Write(sizeSpan);
            writer.Commit();
        }
    }
    public class CustomModelSerializer : IGenericBsonSerializer<CustomModel>
    {
        private static ReadOnlySpan<byte> CustomModelA => new byte[1] { 65 };
        private static ReadOnlySpan<byte> CustomModelB => new byte[1] { 66 };
        private static ReadOnlySpan<byte> CustomModelC => new byte[1] { 67 };
        public bool TryParseBson(ref BsonReader reader, [MaybeNullWhen(false)] out CustomModel message)
        {
            message = default;
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
                throw new Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(CustomModel), endMarker);
            }

            message = new CustomModel(A: Int32A, B: Int32B, C: Int32C);
            return true;
        }

        public void WriteBson(ref BsonWriter writer, in CustomModel message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            writer.Write_Type_Name_Value(CustomModelA, message.A);
            writer.Write_Type_Name_Value(CustomModelB, message.B);
            writer.Write_Type_Name_Value(CustomModelC, message.C);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            Span<byte> sizeSpan = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);
            reserved.Write(sizeSpan);
            writer.Commit();
        }
    }

    [BsonSerializable]
    public partial record ModelWithCustom(string Name, CustomModel Custom, CustomModel2 Custom2);
}
