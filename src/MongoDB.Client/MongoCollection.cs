using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;

namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        private readonly ILogger _logger;
        private readonly Channel _channel;

        internal MongoCollection(MongoDatabase database, string name, ILogger logger)
        {
            _logger = logger;
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
            _channel = new Channel(database.Client.EndPoint, _logger);
        }

        internal void BeginConnection()
        {
            _ = _channel.InitConnectAsync(default);
        }
        
        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }


        
        public async ValueTask<CursorResult<T>> GetCursorAsync(BsonDocument filter, CancellationToken cancellationToken)
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
            return await _channel.GetCursorAsync<T>(request, cancellationToken);
        }

        private MsgMessage CreateFindRequest(string database, BsonDocument document)
        {
            var requestNumber = _channel.GetNextRequestNumber();
            return new MsgMessage(requestNumber, database, document);
        }
    }
}
