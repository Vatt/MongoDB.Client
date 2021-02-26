using MongoDB.Client.Tests.Models;
using System.Threading.Tasks;
using Xunit;
using CommonModel = MongoDB.Client.Tests.Models.CommonModel;

namespace MongoDB.Client.Tests.Client
{
    public class ClientCommonTest : ClientTestBase
    {
        [Fact]
        public async Task CommonTest()
        {
            var model = CommonModel.Create();
            var result = await CreateCollectionInsertFindDeleteDropCollectionAsync(model);
        }
        [Fact]
        public async Task CustomTest()
        {
            var model = CustomModel.Create();
            var result = await CreateCollectionInsertFindDeleteDropCollectionAsync(model);
        }
    }
}
