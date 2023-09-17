using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class ClientReplsSetUpdateTest : ClientUpdateTestBase
    {
        [Fact]
        public async Task ReplsSetUpdateOne_Set_UpdateDoc()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Set_UpdateDoc(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Set_UpdateDoc()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Set_UpdateDoc(client);
        }
        [Fact]
        public async Task ReplSetUpdateDocuments_Insert_Find_UpdateOneSet_SameModel()
        {
            var client = await CreateReplSetClient(1);
            await UpdateDocuments_Insert_Find_UpdateOneSet_SameModel(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Inc()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Inc(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Inc()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Inc(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Mul()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Mul(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Mul()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Mul(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Max_Ok()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Max_Ok(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Max_Ok()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Max_Ok(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Max_Fail()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Max_Fail(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Max_Fail()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Max_Fail(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Min_Ok()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Min_Ok(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Min_Ok()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Min_Ok(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Min_Fail()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Min_Fail(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Min_Fail()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Min_Fail(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_SetOnInsert()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_SetOnInsert(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_SetOnInsert()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_SetOnInsert(client);
        }
        [Fact]
        public async Task ReplSetUpdateOne_Rename()
        {
            var client = await CreateReplSetClient(1);
            await UpdateOne_Rename(client);
        }
        [Fact]
        public async Task ReplSetUpdateMany_Rename()
        {
            var client = await CreateReplSetClient(1);
            await UpdateMany_Rename(client);
        }
    }
}
