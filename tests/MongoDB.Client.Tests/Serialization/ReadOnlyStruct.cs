using System.Threading.Tasks;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class ReadOnlyStruct : SerializationTestBase
    {
        [Fact]
        public async Task ReadOnlyStructTest()
        {
            var model = new ReadonlyStruct(42, 42, "42");
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
    }
}
