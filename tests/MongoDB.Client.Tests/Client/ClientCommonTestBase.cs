using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class ClientCommonTestBase : ClientTestBase
    {
        protected async Task InsertTest(MongoClient client)
        {
            var model = CommonModel.Create();
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<CommonModel>("InsertTestCollection" + DateTimeOffset.UtcNow);
            var result = await InsertAsync(new[] {model}, collection);
            Assert.True(result.Count == 1);
            Assert.Equal(model, result[0]);
            await collection.DropAsync();
        }

        protected async Task FindTest(MongoClient client)
        {
            var model = CommonModel.Create();
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<CommonModel>("InsertTestCollection" + DateTimeOffset.UtcNow);
            var result = await FindAsync(new[] {model}, BsonDocument.Empty, collection);
            Assert.True(result.Count == 1);
            Assert.Equal(model, result[0]);
            await collection.DropAsync();
        }

        protected async Task UpdateOneTest(MongoClient client)
        {
            var model = CommonModel.Create();
            var items = new[] {model, model};
            var update = new BsonDocument("$set", new BsonDocument("StringField", "UPDATED"));
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<CommonModel>("UpdateOneCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await UpdateOneAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(before.SequenceEqual(items));
            Assert.Equal(1, result.Modified);
            Assert.Equal(1, result.N);
            Assert.Equal(1, after.Count(x => x.StringField.Equals("UPDATED")));
            Assert.Equal(1, after.Count(x => x.StringField.Equals("42")));
        }
        protected async Task UpdateManyTest(MongoClient client)
        {
            var model = CommonModel.Create();
            var items = new[] {model, model, model, model, model, model, model, model, model, model, model};
            var update = new BsonDocument("$set", new BsonDocument("StringField", "UPDATED"));
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<CommonModel>("UpdateManyCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await UpdateManyAsync(items, BsonDocument.Empty, update, collection);
            Assert.True(before.SequenceEqual(items));
            Assert.Equal(before.Count, result.Modified);
            Assert.Equal(before.Count, result.N);
            Assert.Equal(before.Count, after.Count(x => x.StringField.Equals("UPDATED")));
            await collection.DropAsync();
        }
        protected async Task DeleteOneTest(MongoClient client)
        {
            var model = CommonModel.Create();
            var items = new[] {model, model};
            var delete = BsonDocument.Empty;
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<CommonModel>("DeleteOneCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await DeleteOneAsync(items, delete, collection);
            Assert.True(before.SequenceEqual(items));
            Assert.Equal(1, result.N);
            Assert.True(after.Count == 1);
            await collection.DropAsync();
        }
        protected async Task DeleteManyTest(MongoClient client)
        {
            var model = CommonModel.Create();
            var items = new[] {model, model, model, model, model, model, model, model, model, model, model};
            var delete = BsonDocument.Empty;
            var db = client.GetDatabase(DB);
            var collection = db.GetCollection<CommonModel>("DeleteManyCollection" + DateTimeOffset.UtcNow);
            var (result, before, after) = await DeleteManyAsync(items, delete, collection);
            Assert.True(before.SequenceEqual(items));
            Assert.Equal(before.Count, result.N);
            Assert.True(after.Count == 0);
            await collection.DropAsync();
        }
    }
}
