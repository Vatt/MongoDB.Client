using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial record GetMoreTestModel(string A, string B, string C, int D, int E);
    public class ClientGetMoreTest : ClientTestBase
    {
        [Fact]
        public async Task StandaloneGetMoreTest()
        {
            var item = new GetMoreTestModel("GetMoreTestModelA", "GetMoreTestModelB", "GetMoreTestModelC", 42, 42);
            var items = new GetMoreTestModel[1024];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = item;
            }
            var client = await CreateStandaloneClient(1);
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<GetMoreTestModel>("GetMoreCollection" + DateTimeOffset.UtcNow);
            var result = await FindAsync(items, BsonDocument.Empty, collection);
            Assert.True(items.Length == result.Count);
            foreach(var resultItem in result)
            {
                Assert.Equal(item, resultItem);
            }

            await collection.DropAsync();
        }
        [Fact]
        public async Task ReplSetGetMoreTest()
        {
            var item = new GetMoreTestModel("GetMoreTestModelA", "GetMoreTestModelB", "GetMoreTestModelC", 42, 42);
            var items = new GetMoreTestModel[1024];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = item;
            }
            var client = await CreateReplSetClient(1, "rs0");
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<GetMoreTestModel>("GetMoreCollection" + DateTimeOffset.UtcNow);

            var result = await FindAsync(items, BsonDocument.Empty, collection);
            Assert.True(items.Length == result.Count);
            foreach(var resultItem in result)
            {
                Assert.Equal(item, resultItem);
            }
            await collection.DropAsync();
        }
    }
}
