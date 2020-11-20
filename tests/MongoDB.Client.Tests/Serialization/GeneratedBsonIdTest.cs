using System;
using System.Threading.Tasks;
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
    }
}