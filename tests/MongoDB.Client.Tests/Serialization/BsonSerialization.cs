using System;
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

