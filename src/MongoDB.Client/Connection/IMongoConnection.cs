using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Messages;

namespace MongoDB.Client.Connection
{
    public interface IMongoConnection
    {
        ValueTask DisposeAsync();
        ValueTask<QueryResult<TResp>> SendQueryAsync<TResp>(string database, BsonDocument document, CancellationToken cancellationToken) where TResp : IBsonSerializer<TResp>;
    }
}
