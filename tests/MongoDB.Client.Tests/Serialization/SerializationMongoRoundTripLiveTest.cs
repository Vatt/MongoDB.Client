using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class SerializationMongoRoundTripLiveTest : SerializationTestBase
    {
        [Fact]
        public async Task MongoDBRoundTripAsync_UsesAuthAwareDefaultsForLocalDocker()
        {
            var model = CommonModel.Create();

            var result = await MongoDBRoundTripAsync(model);

            Assert.NotNull(result);
            Assert.Equal(model, result);
        }
    }
}
