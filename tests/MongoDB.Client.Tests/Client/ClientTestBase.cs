using System.Net;
using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Tests.Client
{
    public abstract class ClientTestBase
    {
        protected string StandaloneHost { get; init; } = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
        protected string RsHost { get; init; } = Environment.GetEnvironmentVariable("MONGODB_RS_HOST") ?? "localhost";
        protected string ShardedHost { get; init; } = Environment.GetEnvironmentVariable("MONGODB_SHARDED_HOST") ?? "localhost";
        
        protected string DB { get; init; } = "TestDb";
        protected string Collection { get; init; } = "TestCollection";

        protected async Task<List<T>> CreateCollectionInsertFindDeleteDropCollectionAsync<T>(T[] data, string host, bool useTransaction = false) //where T : IBsonSerializer<T>
        {
            List<T> findResult = default;
            var client = await MongoClient.CreateClient(new DnsEndPoint(host, 27017)).ConfigureAwait(false);
            var collection = client.GetDatabase(DB).GetCollection<T>(Collection + Guid.NewGuid());
            await collection.CreateAsync();
            if (useTransaction)
            {
                var transaction = client.StartTransaction();
                await collection.InsertAsync(transaction, data);
                findResult = await collection.Find(transaction, BsonDocument.Empty).ToListAsync();
                await collection.DeleteOneAsync(transaction, BsonDocument.Empty);
                await transaction.CommitAsync();
            }
            else
            {
                await collection.InsertAsync(data);
                findResult = await collection.Find(BsonDocument.Empty).ToListAsync();
                await collection.DeleteOneAsync(BsonDocument.Empty);
            }

            await collection.DropAsync();
            return findResult;
        }
    }
}
