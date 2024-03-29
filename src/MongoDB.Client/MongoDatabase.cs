﻿using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Scheduler;

namespace MongoDB.Client
{
    public class MongoDatabase
    {
        private readonly IMongoScheduler _scheduler;
        public MongoClient Client { get; }
        public string Name { get; } //TODO: byte[] mb?

        internal MongoDatabase(MongoClient client, string name, IMongoScheduler scheduler)
        {
            _scheduler = scheduler;
            Client = client;
            Name = name;
        }

        public MongoCollection<T> GetCollection<T>(string name)
            where T : IBsonSerializer<T>
        {
            return new MongoCollection<T>(this, name, _scheduler);
        }

        public ValueTask DropCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            return DropCollectionAsync(TransactionHandler.CreateImplicit(_scheduler), collectionName, cancellationToken);
        }

        public ValueTask DropCollectionAsync(TransactionHandler transaction, string collectionName, CancellationToken cancellationToken = default)
        {
            //var collection = GetCollection<object>(collectionName);
            var collection = GetCollection<BsonDocument>(collectionName);
            return collection.DropAsync(transaction, cancellationToken);
        }

        public ValueTask CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
        {
            return CreateCollectionAsync(TransactionHandler.CreateImplicit(_scheduler), collectionName, cancellationToken);
        }

        public ValueTask CreateCollectionAsync(TransactionHandler transaction, string collectionName, CancellationToken cancellationToken = default)
        {
            var collection = GetCollection<BsonDocument>(collectionName);
            return collection.CreateAsync(transaction, cancellationToken);
        }
    }
}
