using System.Net;
using System.Text;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using Xunit.Sdk;

namespace MongoDB.Client.Tests.Client
{
    public abstract class ClientTestBase
    {
        protected string StandaloneHost { get; } = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
        protected string RsHost { get; } = Environment.GetEnvironmentVariable("MONGODB_RS_HOST") ?? "localhost";
        protected string ShardedHost { get; } = Environment.GetEnvironmentVariable("MONGODB_SHARDED_HOST") ?? "localhost";
        
        protected string DB { get; init; } = "TestDb";
        protected string Collection { get; init; } = "TestCollection";

        protected async Task<List<T>> InsertAsync<T>(IEnumerable<T> items, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
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
        
        protected async Task<(UpdateResult result, List<T> before, List<T> after)> UpdateOneAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, BsonDocument update, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
        {
            UpdateResult result = default;
            List<T> after = default;
            List<T> before = default;
            if (tx != null)
            {
                before =  await InsertAsync(insertItems, collection, tx, txCommit);
                result = await collection.UpdateOneAsync(tx, filter, update);
                after = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                before =  await InsertAsync(insertItems, collection);
                result = await collection.UpdateOneAsync(filter, update);
                after = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return (result, before, after);
        }
        protected async Task<(UpdateResult result, List<T> before, List<T> after)> UpdateManyAsync<T>(IEnumerable<T> insertItems, BsonDocument filter, BsonDocument update, MongoCollection<T> collection, TransactionHandler? tx = null, bool txCommit = false)
        {
            UpdateResult result = default;
            List<T> after = default;
            List<T> before = default;
            if (tx != null)
            {
                before =  await InsertAsync(insertItems, collection, tx, txCommit);
                result = await collection.UpdateManyAsync(tx, filter, update);
                after = await collection.Find(tx, BsonDocument.Empty).ToListAsync();
                if (txCommit)
                {
                    await tx.CommitAsync();
                }
            }
            else
            {
                before =  await InsertAsync(insertItems, collection);
                result = await collection.UpdateManyAsync(filter, update);
                after = await collection.Find(BsonDocument.Empty).ToListAsync();
            }
            return (result, before, after);
        }

        protected Task<MongoClient> CreateStandaloneClient(int connPoolSize)
        {
            var connectionStr = $"mongodb://{StandaloneHost}/?maxPoolSize={connPoolSize}";
            return MongoClient.CreateClient(connectionStr);
        }

        protected Task<MongoClient> CreateReplSetClient(int connPoolSize, string rsName)
        {
            var connectionStr = $"mongodb://{RsHost}/?replicaSet={rsName}&maxPoolSize={connPoolSize}";
            return MongoClient.CreateClient(connectionStr);
        }
    }
}
