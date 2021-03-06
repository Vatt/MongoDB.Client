using System.Threading.Tasks;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class IfvsSwitchTest : BaseSerialization
    {
        [Fact]
        public async Task IfvsSwitchVerifyTest()
        {
            var ifModel = IfShortNamesModel.Create();
            var ifResult = await RoundTripAsync(ifModel);
            Assert.Equal(ifModel, ifResult);
        }
        [Fact]
        public async Task SwitchShortVerifyTest()
        {
            var switchShortModel = SwitchShortNamesModel.Create();
            var switchResult = await RoundTripAsync(switchShortModel);
            Assert.Equal(switchShortModel, switchResult);
        }
    }
}