using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class ClientStandaloneUpdateTest : ClientUpdateTestBase
    {
        [Fact]
        public async Task StandaloneUpdateOne_Set_UpdateDoc()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Set_UpdateDoc(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Set_UpdateDoc()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Set_UpdateDoc(client);
        }
        [Fact]
        public async Task StandaloneUpdateDocuments_Insert_Find_UpdateOneSet_SameModel()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateDocuments_Insert_Find_UpdateOneSet_SameModel(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_Inc()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Inc(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Inc()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Inc(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_Mul()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Mul(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Mul()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Mul(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_Max_Ok()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Max_Ok(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Max_Ok()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Max_Ok(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_Max_Fail()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Max_Fail(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Max_Fail()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Max_Fail(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_Min_Ok()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Min_Ok(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Min_Ok()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Min_Ok(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_Min_Fail()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_Min_Fail(client);
        }
        [Fact]
        public async Task StandaloneUpdateMany_Min_Fail()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateMany_Min_Fail(client);
        }
        [Fact]
        public async Task StandaloneUpdateOne_SetOnInsert()
        {
            var client = await CreateStandaloneClient(1);
            await UpdateOne_SetOnInsert(client);
        }
    }
}
