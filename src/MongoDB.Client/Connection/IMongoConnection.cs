using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Connection
{
    public interface IMongoConnection
    {
        ValueTask DisposeAsync();
        ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken);
    }
}