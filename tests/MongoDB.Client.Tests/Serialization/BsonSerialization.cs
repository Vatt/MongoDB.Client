using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests
{
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
            TestData doc = new TestData
            {
                Age = 42,
                Id = new BsonObjectId(42, 42, 42),
                Name = "DATA_TEST_STRING_NAME",
                InnerData = new TestData.InnerTestData
                {
                    Value0 = 42,
                    Value1 = 42,
                    Value2 = 42,
                }
            };
            SerializersMap.TryGetSerializer<TestData>(out var serializer);
            var result = await RoundTripAsync(doc, serializer);


            Assert.Equal(doc, result);
        }
    }

}

