using System.Threading.Tasks;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class ByteArrayTest : BaseSerialization
    {
        [Fact]
        public async Task ByteArrayAndMemoryByteTest()
        {
            var model = ByteArrayModel.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
    }
}
