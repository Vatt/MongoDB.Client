using MongoDB.Client.Tests.Serialization.TestModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedNullCheckTest : BaseSerialization
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