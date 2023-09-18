using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public partial class CollectionTests : ClientTestBase
    {
        [BsonSerializable]
        public partial record TestBson(int A);
        [Fact]
        public async Task StandaloneCreateCollectionTest()
        {
            var client = await CreateStandaloneClient(1);
            var db = client.GetDatabase(DB);
            await db.CreateCollectionAsync($"StandaloneCreateCollectionTest");
            var collection = db.GetCollection<TestBson>(DB);
            var result = await InsertAsync(new List<TestBson> { new(1) }, collection);
            Assert.Single(result);
            Assert.Equal(1, result[0].A);
        }
    }
}
