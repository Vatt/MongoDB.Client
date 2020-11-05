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
        private readonly ChannelsPool _channelsPool;
        private readonly ILoggerFactory _loggerFactory;

        internal MongoCollection(MongoDatabase database, string name, ChannelsPool channelsPool,
            ILoggerFactory loggerFactory)
        {
            _channelsPool = channelsPool;
            _loggerFactory = loggerFactory;
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
        }
        
        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }

        
        public async ValueTask<CursorResult<T>> GetCursorAsync(BsonDocument filter, CancellationToken cancellationToken = default)
        {
            var doc = new BsonDocument
            {
                {"find", Namespace.CollectionName },
                {"filter", filter },
                {"$db", Database.Name},
                {"lsid", new BsonDocument("id", BsonBinaryData.Create(Guid.NewGuid())) }
            };
            var channel = await _channelsPool.GetChannelAsync(cancellationToken).ConfigureAwait(false);
            var requestNum = channel.GetNextRequestNumber();
            var request = CreateFindRequest(Database.Name, doc, requestNum);
            return await channel.GetCursorAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        public Cursor<T> Find(BsonDocument filter)
        {
            return new Cursor<T>(_channelsPool, filter, Namespace);
        }
        
        private MsgMessage CreateFindRequest(string database, BsonDocument document, int requestNum)
        {
            return new MsgMessage(requestNum, database, document);
        }
    }
}
