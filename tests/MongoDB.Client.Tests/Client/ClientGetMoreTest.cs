using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial record GetMoreTestModel(string A, string B, string C, int D, int E);
    public class ClientGetMoreTest : ClientTestBase
    {
        [Fact]
        public async Task GetMoreTest()
        {
            var item = new GetMoreTestModel("GetMoreTestModelA", "GetMoreTestModelB", "GetMoreTestModelC", 42, 42);
            var items = new GetMoreTestModel[1024];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = item;
            }
            
            var result = await CreateCollectionInsertFindDeleteDropCollectionAsync(items, StandaloneHost);
            Assert.True(items.Length == result.Count);
            foreach(var resultItem in result)
            {
                Assert.Equal(item, resultItem);
            }

            result = await CreateCollectionInsertFindDeleteDropCollectionAsync(items, RsHost);
            Assert.True(items.Length == result.Count);
            foreach (var resultItem in result)
            {
                Assert.Equal(item, resultItem);
            }
        }
    }
}
