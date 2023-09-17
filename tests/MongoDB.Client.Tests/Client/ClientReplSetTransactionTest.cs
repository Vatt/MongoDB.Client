using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    [BsonSerializable]
    public partial record TransactionTestModel(string A, string B, string C, int D, int E);
    public class ClientReplSetTransactionTest : ClientCommonTestBase
    {
        [Fact]
        public async Task ReplSetTransactionSimpleTest()
        {
            var name = "TransactionSimple" + DateTimeOffset.UtcNow;
            var item = new TransactionTestModel("TransactionTestModelA", "TransactionTestModelB", "TransactionTestModelC", 42, 42);
            var update = Update.Set(new TransactionTestModel("UPDATED", "TransactionTestModelB", "TransactionTestModelC", 42, 42));
            var items = new TransactionTestModel[1024];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = item;
            }

            var client = await CreateReplSetClient(4, RsName);
            var db = client.GetDatabase(DB);
            var tx = client.StartTransaction();
            await db.CreateCollectionAsync(tx, name);
            var collection = db.GetCollection<TransactionTestModel>(name);
            await collection.InsertAsync(tx, items);
            var findResult = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
            Assert.True(items.SequenceEqual(findResult));
            var updateResult = await collection.UpdateManyAsync(tx, BsonDocument.Empty, update);
            Assert.Equal(items.Length, updateResult.N);
            Assert.Equal(items.Length, updateResult.Modified);
            var afterUpdateFindReuslt = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
            foreach (var afterUpdateItem in afterUpdateFindReuslt)
            {
                Assert.True(afterUpdateItem.A.Equals("UPDATED"));
            }
            var deleteResult = await collection.DeleteManyAsync(tx, BsonDocument.Empty);
            Assert.Equal(items.Length, deleteResult.N);
            var afterDeleteFindResult = await collection.Find(BsonDocument.Empty).ToListAsync();
            Assert.True(afterDeleteFindResult.Count == 0);
            //await db.DropCollectionAsync(tx, name);
            await tx.CommitAsync();
            await collection.DropAsync();

        }
    }
}
