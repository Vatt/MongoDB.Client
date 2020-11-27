using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedBsonIdTest : BaseSerialization
    {
        [Fact]
        public async Task BsonIdTest()
        {
            var model = new BsonIdModel(Guid.NewGuid(), Int32.MaxValue);
            SerializersMap.TryGetSerializer<BsonIdModel>(out var serializer);
            var result = await RoundTripAsync(model, serializer);

            Assert.Equal(model, result);
        }


        [Fact]
        public async Task GenerateBsonObjectIdTest()
        {
            var model = new BsonObjectIdModel
            {
                SomeInt = 42
            };
            SerializersMap.TryGetSerializer<BsonObjectIdModel>(out var serializer);
            var result = await RoundTripAsync(model, serializer);

            Assert.True(model.Id != default);
            Assert.Equal(model, result);
        }
    }
}