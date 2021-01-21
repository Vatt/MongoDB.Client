using MongoDB.Client.Tests.Serialization.TestModels;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedCustomModelTest : BaseSerialization
    {
        [Fact]
        public async Task CustomModelTest()
        {
            var model = new ModelWithCustom("CustomModelTest", new CustomModel(42, 42, 42), new CustomModel2(24, 24, 24));

            var result = await RoundTripAsync(model);
            Assert.Equal(result, model);
        }
    }
}
