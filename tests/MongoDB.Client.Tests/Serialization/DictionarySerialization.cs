using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class DictionarySerialization : BaseSerialization
    {
        [Fact]
        public async Task DictionarySerializationTest()
        {
            var model = new ModelWithDictionary
            {
                Value = "string value",
                Data = new Dictionary<string, string>
                {
                    ["key1"] = "value1",
                    ["key2"] = "value2",
                    ["key3"] = "value3",
                },
                NullableData = new Dictionary<string, int?>
                {
                ["key1"] = 1,
                ["key2"] = 2,
                ["key3"] = 3,
            }
            };
            var result = await RoundTripAsync(model);

            Assert.Equal(model, result);
        }
    }

    [BsonSerializable]
    public partial class ModelWithDictionary : IEquatable<ModelWithDictionary>
    {
        public string Value { get; set; }

        //[BsonSerializer(typeof(DictionarySerializer))]
        public Dictionary<string, string> Data { get; set; }
        public Dictionary<string, int?>? NullableData { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ModelWithDictionary dictionary && Equals(dictionary);

        }

        public bool Equals(ModelWithDictionary other)
        {
            if (Value != other.Value)
            {
                return false;
            }
            if (Data.Count != other.Data.Count && NullableData!.Count != other.NullableData!.Count)
            {
                return false;
            }

            foreach (var (key, value) in Data)
            {
                if (other.Data.TryGetValue(key, out var otherValue) == false || value != otherValue)
                {
                    return false;
                }
            }
            foreach (var (key, value) in NullableData!)
            {
                if (other.NullableData!.TryGetValue(key, out var otherValue) == false || value != otherValue)
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Data, NullableData);
        }
    }

    public class DictionarySerializer
    {
        public static bool TryParseBson(ref BsonReader reader, [MaybeNullWhen(false)] out Dictionary<string, string> message)
        {
            message = default;
            if (!reader.TryGetInt32(out int docLength))
            {
                return false;
            }
            var dick = new Dictionary<string, string>();
            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                if (!reader.TryGetByte(out var bsonType))
                {
                    return false;
                }

                if (!reader.TryGetCString(out var key))
                {
                    return false;
                }

                if (bsonType == 10)
                {
                    dick.Add(key, null);
                    continue;
                }

                if (!reader.TryGetString(out var value))
                {
                    return false;
                }
                dick.Add(key, value);
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(Dictionary<string, string>), endMarker);
            }

            message = dick;
            return true;
        }

        public static void WriteBson(ref BsonWriter writer, in Dictionary<string, string> message, out byte bsonType)
        {
            bsonType = 3;
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            foreach (var (key, value) in message)
            {
                writer.Write_Type_Name_Value(key, value);
            }
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }
    }
}
