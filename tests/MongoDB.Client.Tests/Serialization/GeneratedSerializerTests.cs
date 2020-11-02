using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedSerializerTests : BaseSerialization
    {
        [Fact]
        public async Task SerializationDeserializationGenerated()
        {
            var doc = new TestData
            {
                Age = 42,
                Id = new BsonObjectId("5f987814bf344ec7cc57294b"),
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

        [Fact]
        public async Task IgnoreTest()
        {
            var doc = new ModelWithIgnore
            {
                Field = "Field",
                IgnoredField = "IgnoredField"
            };
            SerializersMap.TryGetSerializer<ModelWithIgnore>(out var serializer);


            var result = await RoundTripAsync(doc, serializer);


            Assert.Equal(doc.Field, result.Field);
            Assert.Null(result.IgnoredField);
        }
        
        
        [Fact]
        public async Task ArrayTest()
        {
            var doc = new ModelWithArray
            {
                Name = "SomeName",
                Values = new List<int> {1, 2, 3, 4, 5, 6}
            };
            SerializersMap.TryGetSerializer<ModelWithArray>(out var serializer);


            var result = await RoundTripAsync(doc, serializer);


            Assert.Equal(doc, result);
        }
    }
}