using System;
using System.Threading.Tasks;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedBsonIdTest : SerializationTestBase
    {
        [Fact]
        public async Task BsonIdTest()
        {
            var model = new BsonIdModel(Guid.NewGuid(), Int32.MaxValue);
            var result = await RoundTripAsync(model);

            Assert.Equal(model, result);
        }


        [Fact]
        public async Task GenerateBsonObjectIdTest()
        {
            var model = new BsonObjectIdModel
            {
                SomeInt = 42
            };
            var result = await RoundTripAsync(model);

            Assert.True(model.Id != default);
            Assert.Equal(model, result);
        }
    }
}