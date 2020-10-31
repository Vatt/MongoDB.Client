using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        private readonly Channel _channel;

        internal MongoCollection(MongoDatabase database, string name)
        {
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
            _channel = new Channel(database.Client.EndPoint);
        }

        internal void BeginConnection()
        {
            _ = _channel.InitConnectAsync(default);
        }
        
        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }


        
        public async ValueTask<CursorResult<TResp>> GetCursorAsync<TResp>(BsonDocument filter, CancellationToken cancellationToken)
        {
            if (_channel.Init == false)
            {
                await _channel.InitConnectAsync(cancellationToken).ConfigureAwait(false);
            }
            var doc = new BsonDocument
            {
                {"find", Namespace.CollectionName },
                {"filter", filter },
                {"$db", Database.Name},
                {"lsid", new BsonDocument("id", BsonBinaryData.Create(Guid.NewGuid())) }
            };
            
            var request = CreateFindRequest(Database.Name, doc);
            return await _channel.GetCursorAsync<TResp>(request, cancellationToken);
        }

        private MsgMessage CreateFindRequest(string database, BsonDocument document)
        {
            var requestNumber = _channel.GetNextRequestNumber();
            return new MsgMessage(requestNumber, database, document);
        }
    }
}
