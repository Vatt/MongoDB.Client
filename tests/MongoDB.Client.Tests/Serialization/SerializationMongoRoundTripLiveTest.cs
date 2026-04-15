using MongoDB.Client.Tests.Models;
using MongoDB.Client.Tests.Infrastructure;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class SerializationMongoRoundTripLiveTest : SerializationTestBase
    {
        [RequiresStandaloneMongoFact]
        public async Task MongoDBRoundTripAsync_UsesAuthAwareDefaultsForLocalDocker()
        {
            var model = CommonModel.Create();

            var result = await MongoDBRoundTripAsync(model);

            Assert.NotNull(result);
            Assert.Equal(model, result);
        }

        [RequiresStandaloneMongoFact]
        public async Task MongoDBRoundTripAsync_IsStableAcrossSequentialRuns()
        {
            var firstModel = CommonModel.Create();
            var secondModel = CommonModel.Create();

            var firstResult = await MongoDBRoundTripAsync(firstModel);
            var secondResult = await MongoDBRoundTripAsync(secondModel);

            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.Equal(firstModel, firstResult);
            Assert.Equal(secondModel, secondResult);
        }
    }
}
