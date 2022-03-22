using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class ClientReplSetCommonTest : ClientCommonTestBase
    {
        [Fact]
        public async Task ReplSetInsertTest()
        {
            var client = await CreateReplSetClient(1, RsName);
            await InsertTest(client);
        }

        [Fact]
        public async Task ReplSetFindTest()
        {
            var client = await CreateReplSetClient(1, RsName);
            await FindTest(client);
        }

        [Fact]
        public async Task ReplSetUpdateOneTest()
        {
            var client = await CreateReplSetClient(1, RsName);
            await UpdateOneTest(client);
        }

        [Fact]
        public async Task ReplSetUpdateManyTest()
        {
            var client = await CreateReplSetClient(1, RsName);
            await UpdateManyTest(client);
        }

        [Fact]
        public async Task ReplSetDeleteOneTest()
        {
            var client = await CreateReplSetClient(1, RsName);
            await DeleteOneTest(client);
        }

        [Fact]
        public async Task ReplSetDeleteManyTest()
        {
            var client = await CreateReplSetClient(1, RsName);
            await DeleteManyTest(client);
        }
    }
}
