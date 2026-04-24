using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;
using MongoDB.Client.Tests.Infrastructure;

namespace MongoDB.Client.Tests.Client
{
    public abstract class ClientTestBase
    {
        protected string StandaloneHost { get; } = IntegrationMongoConnectionStringBuilder.StandaloneHost;
        protected string RsHost { get; } = IntegrationMongoConnectionStringBuilder.ReplicaSetHost;
        protected string ShardedHost { get; } = IntegrationMongoConnectionStringBuilder.ShardedHost;

        protected string DB { get; init; } = "TestDb";
        protected string Collection { get; init; } = "TestCollection";

        protected readonly string RsName = IntegrationMongoConnectionStringBuilder.ReplicaSetName;
        protected async Task<List<T>> InsertAsync<T>(IEnumerable<T> items, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
            where T : IBsonSerializer<T>
        {
            List<T> result = default;
            if (tx != null)
            {
                await collection.InsertAsync(tx, items);
                result = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }

            }
            else
            {
                await collection.InsertAsync(items);
                result = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return result;
        }
        protected async Task<List<T>> FindAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
            where T : IBsonSerializer<T>
        {
            List<T> result = default;
            if (tx != null)
            {
                await collection.InsertAsync(tx, insertItems);
                result = await collection.Find(tx, filter).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                await collection.InsertAsync(insertItems);
                result = await collection.Find(filter).ToListAsync();
            }
            return result;
        }

        protected async Task<(DeleteResult result, List<T> before, List<T> after)> DeleteOneAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
            where T : IBsonSerializer<T>
        {
            DeleteResult result = default;
            List<T> after = default;
            List<T> before = default;
            if (tx != null)
            {
                before = await InsertAsync(insertItems, collection, tx, txCommit);
                result = await collection.DeleteOneAsync(tx, filter);
                after = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                before = await InsertAsync(insertItems, collection);
                result = await collection.DeleteOneAsync(filter);
                after = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return (result, before, after);
        }
        protected async Task<(DeleteResult result, List<T> before, List<T> after)> DeleteManyAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
            where T : IBsonSerializer<T>
        {
            DeleteResult result = default;
            List<T> after = default;
            List<T> before = default;
            if (tx != null)
            {
                before = await InsertAsync(insertItems, collection, tx, txCommit);
                result = await collection.DeleteManyAsync(tx, filter);
                after = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                before = await InsertAsync(insertItems, collection);
                result = await collection.DeleteManyAsync(filter);
                after = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return (result, before, after);
        }

        protected async Task<(UpdateResult result, List<T> before, List<T> after)> UpdateOneAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, Update update, MongoCollection<T> collection, UpdateOptions? options = null, TransactionHandler? tx = null, bool txCommit = false)
            where T : IBsonSerializer<T>
        {
            UpdateResult result = default;
            List<T> after = default;
            List<T> before = default;
            if (tx != null)
            {
                before = await InsertAsync(insertItems, collection, tx, txCommit);
                result = await collection.UpdateOneAsync(tx, filter, update, options);
                after = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                before = await InsertAsync(insertItems, collection);
                result = await collection.UpdateOneAsync(filter, update, options);
                after = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return (result, before, after);
        }
        protected async Task<(UpdateResult result, List<T> before, List<T> after)> UpdateManyAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, Update update, MongoCollection<T> collection, UpdateOptions? options = null, TransactionHandler? tx = null, bool txCommit = false)
             where T : IBsonSerializer<T>
        {
            UpdateResult result = default;
            List<T> after = default;
            List<T> before = default;
            if (tx != null)
            {
                before = await InsertAsync(insertItems, collection, tx, txCommit);
                result = await collection.UpdateManyAsync(tx, filter, update, options);
                after = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                before = await InsertAsync(insertItems, collection);
                result = await collection.UpdateManyAsync(filter, update, options);
                after = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return (result, before, after);
        }

        protected Task<MongoClient> CreateStandaloneClient(int connPoolSize)
        {
            var connectionStr = IntegrationMongoConnectionStringBuilder.BuildStandalone(connPoolSize);
            return MongoClient.CreateClient(connectionStr);
        }

        protected Task<MongoClient> CreateReplSetClient(int connPoolSize, string? rsName = null)
        {
            var connectionStr = IntegrationMongoConnectionStringBuilder.BuildReplicaSet(connPoolSize, rsName);
            return MongoClient.CreateClient(connectionStr);
        }
    }
}
