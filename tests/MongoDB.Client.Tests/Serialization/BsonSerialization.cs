using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
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
                { "objectId", new BsonObjectId("5f987814bf344ec7cc57294b")},
                {"array", new  BsonArray { "item1", default(string), 42, true } },
                { "inner", new BsonDocument {
                    {"innerString", "inner string" }
                } }
            };

            var result = await RoundTripAsync(doc, new BsonDocumentSerializer());

            Assert.Equal(doc, result);
        }


        [Fact]
        public async Task GuidSerializationDeserialization()
        {
            var guid = Guid.NewGuid();
            var doc = new BsonDocument
            {
                { "guid", BsonBinaryData.Create(guid)}
            };

            var result = await RoundTripAsync(doc, new BsonDocumentSerializer());

            Assert.Equal(doc, result);
        }

        [Fact]
        public async Task ObjectIdBsonSerializationDeserialization()
        {
            var oid = new BsonObjectId("5f987814bf344ec7cc57294b");
            var doc = new BsonDocument
            {
                { "objectId", oid}
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
                Id = new BsonObjectId("5f987814bf344ec7cc57294b"),
                Name = "DATA_TEST_STRING_NAME",
                InnerData = new TestData.InnerTestData
                {
                    Value0 = 42,
                    Value1 = 42,
                    Value2 = 42,
                    IntList = new List<int>() { 1024, 1025, 1026, 1027, 1028, 1029 },
                    LongList = new List<long>() { 1024, 1025, 1026, 1027, 1028, 1029 },
                    DoubleList = new List<double>() { 42.42, 425.42, 42.42 },
                    StringList = new List<string>() { "42", "42", "42" },
                    BoolList = new List<bool>() { true, false, true },
                    BsonDocumentList = new List<BsonDocument>() { new BsonDocument { { "int", 42 }, { "bool", true }, { "string1", "string" }, { "string2", "" }, { "string3", default(string) }}},
                    BsonObjectIdList = new List<BsonObjectId>() { new BsonObjectId("5f987814bf344ec7cc57294b"), new BsonObjectId("5f987814bf342ec7cc57294b") },
                    //DateTimeOffsetList = new List<DateTimeOffset>() { DateTimeOffset.UtcNow, DateTimeOffset.UtcNow }
                }
            };
            SerializersMap.TryGetSerializer<TestData>(out var serializer);
            var result = await RoundTripAsync(doc, serializer);


            Assert.Equal(doc, result);
        }
        
        
        [Fact]
        public void ObjectIdSerializationDeserialization()
        {
            var buffer = new byte[12];
            var oid = new BsonObjectId("5f987814bf344ec7cc57294b");

            oid.TryWriteBytes(buffer);
            var newOid = new BsonObjectId(buffer);

            Assert.Equal(oid, newOid);
        }
    }

}

