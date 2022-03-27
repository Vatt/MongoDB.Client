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
    }
}
