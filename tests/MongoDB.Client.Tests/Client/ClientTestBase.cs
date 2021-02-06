using MongoDB.Client.Bson.Document;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MongoDB.Client.Tests.Client
{
    public abstract class ClientTestBase
    {
        protected string Host { get; init; } = Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost";
        protected string DB { get; init; } = "TestDb";
        protected string Collection { get; init; } = "TestCollection";
        protected MongoClient Client { get; init; }
        public ClientTestBase()
        {
            Client = new MongoClient(new DnsEndPoint(Host, 27017));
        }
        protected async Task<T> InsertFindDeleteAsync<T>(T data)
        {
            await Client.InitAsync();
            var collection = Client.GetDatabase(DB).GetCollection<T>(Collection);
            await collection.CreateAsync();
            await collection.InsertAsync(data);
            var findResult = await collection.Find(BsonDocument.Empty).FirstOrDefaultAsync();
            await collection.DeleteOneAsync(BsonDocument.Empty);
            await collection.DropAsync();
            return findResult;
        }
    }
}
