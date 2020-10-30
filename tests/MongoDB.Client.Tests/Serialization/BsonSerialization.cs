using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization;
using Xunit;

namespace MongoDB.Client.Tests
{
    [BsonSerializable]
    public class TestData : IEquatable<TestData>
    {
        [BsonElementField(ElementName = "_id")]
        public BsonObjectId Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public TestData InnerData { get; set; }
        public override bool Equals(object? obj)
        {
            return obj is Data data && Equals(data);
        }

        public bool Equals(TestData? other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(other, this))
            {
                return true;
            }
            if (InnerData == null && other.InnerData == null)
            {
                return true;
            }
            if ((InnerData != null && other.InnerData == null) || (InnerData == null && other.InnerData != null))
            {
                return false;
            }
            return EqualityComparer<BsonObjectId>.Default.Equals(Id, other.Id) && Name == other.Name && Age == other.Age && InnerData.Equals(other.InnerData);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name, Age);
        }
    }
    public class BsonSerialization : BaseSerialization
    {
        [Fact]
        public async Task SerializationDeserialization()
        {
            var doc = new BsonDocument
            {
                { "int", 42},
                { "bool", true},
                { "string1", "string"},
                { "string2", ""},
                { "string3", default(string)},
                {"array", new  BsonArray { "item1", default(string), 42, true } },
                { "inner", new BsonDocument {
                    {"innerString", "inner string" }
                } }
            };

            var result = await RoundTripAsync(doc, new BsonDocumentSerializer());

            Assert.Equal(doc, result);
        }


        [Fact]
        public async Task SerializationDeserializationGenerated()
        {
            TestData inner = new TestData
            {
                Age = 24,
                Id = new BsonObjectId(24, 24, 24),
                Name = "INNER_DATA",
                InnerData = null,
            };
            TestData doc = new TestData
            {
                Age = 42,
                Id = new BsonObjectId(42, 42, 42),
                Name = "DATA_TEST_STRING_NAME",
            };
            //inner.InnerData = doc;
            doc.InnerData = inner;
            SerializersMap.TryGetSerializer<TestData>(out var serializer);
            var result = await RoundTripAsync(doc, serializer);


            Assert.Equal(doc, result);
        }
    }

}

