using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using Xunit;
using CommonModel = MongoDB.Client.Tests.Models.CommonModel;

namespace MongoDB.Client.Tests.Client
{
    public class ClientStandaloneCommonTest : ClientCommonTestBase
    {
        [Fact]
        public async Task StandaloneInsertTest()
        {
            var client = await CreateStandaloneClient(1);
            await InsertTest(client);
        }

        [Fact]
        public async Task StandaloneFindTest()
        {
            var client = await CreateStandaloneClient(1);
            await FindTest(client);
        }

        [Fact]
        public async Task StandaloneUpdateOneTest()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOneTest(client);
        }

        [Fact]
        public async Task StandaloneUpdateManyTest()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateManyTest(client);
        }

        [Fact]
        public async Task StandaloneDeleteOneTest()
        {
            var client = await CreateStandaloneClient(1);
            await DeleteOneTest(client);
        }

        [Fact]
        public async Task StandaloneDeleteManyTest()
        {
            var client = await CreateStandaloneClient(1);
            await DeleteManyTest(client);
        }
    }
}
