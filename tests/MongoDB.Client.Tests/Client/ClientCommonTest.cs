using MongoDB.Client.Tests.Models;
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
            var result = await CreateCollectionInsertFindDeleteDropCollectionAsync(new[] { model }, StandaloneHost);
            Assert.True(result.Count == 1);
            Assert.Equal(model, result[0]);
            
            result = await CreateCollectionInsertFindDeleteDropCollectionAsync(new[] { model }, RsHost);
            Assert.True(result.Count == 1);
            Assert.Equal(model, result[0]);
        }
        [Fact]
        public async Task CustomTest()
        {
            var model = CustomModel.Create();
            var result = await CreateCollectionInsertFindDeleteDropCollectionAsync(new[] { model }, StandaloneHost);
            Assert.True(result.Count == 1);
            Assert.Equal(model, result[0]);

            result = await CreateCollectionInsertFindDeleteDropCollectionAsync(new[] { model }, RsHost);
            Assert.True(result.Count == 1);
            Assert.Equal(model, result[0]);
        }
    }
}
