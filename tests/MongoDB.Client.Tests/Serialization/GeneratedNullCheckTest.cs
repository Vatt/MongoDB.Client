using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedNullCheckTest : SerializationTestBase
    {
        [Fact]
        public async Task NullCheckTest()
        {
            var model = new NullCheckModel(null, new List<NullCheckData>()
                {
                    new NullCheckData(42),
                    null,
                    new NullCheckData(42),
                });
            var result = await RoundTripAsync<NullCheckModel>(model);

            Assert.Equal(result, model);
        }
    }
}