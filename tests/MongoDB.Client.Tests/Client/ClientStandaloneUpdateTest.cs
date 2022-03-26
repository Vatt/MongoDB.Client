using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class ClientStandaloneUpdateTest : ClientUpdateTestBase
    {
        [Fact]
        public async Task StandaloneUpdateOne_Set()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Set(client);
        }
    }
}
