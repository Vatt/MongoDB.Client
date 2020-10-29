using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization;
using Xunit;

namespace MongoDB.Client.Tests
{
    [BsonSerializable]
    public class Sample
    {
        public long LongValue;
        public string StringValue;
        public int IntValue;
        public double DoubleValue;
        public DateTimeOffset DateTimeValue { get; set; }
        public BsonObjectId ObjectId;
        public bool BooleanValue;
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

            // TODO: Need to implement BsonDocument equals
            Assert.Equal(doc, result);
        }
        [Fact]
        public async Task SerializationDeserializationGenerated()
        {
            var doc = new Data();
            doc.Age = 42;
            doc.Id = new BsonObjectId(42,42,42);
            doc.Name = "DATA_TEST_STRING_NAME";
            SerializersMap.TryGetSerializer<Data>(out var serializer);
            var result = await RoundTripAsync(doc, serializer);

            // TODO: Need to implement BsonDocument equals
            Assert.Equal(doc, result);
        }
    }

}

