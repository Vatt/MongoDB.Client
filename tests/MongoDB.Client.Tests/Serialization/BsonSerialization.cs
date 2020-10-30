using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization;
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
            var inner = new Data
            {
                Age = 24,
                Id = new BsonObjectId(24, 24, 24),
                Name = "INNER_DATA"
            };
            var doc = new Data
            {
                Age = 42,
                Id = new BsonObjectId(42, 42, 42),
                Name = "DATA_TEST_STRING_NAME",
                InnerData = inner
            };
            SerializersMap.TryGetSerializer<Data>(out var serializer);
            var result = await RoundTripAsync(doc, serializer);


            Assert.Equal(doc, result);
        }
    }

}

