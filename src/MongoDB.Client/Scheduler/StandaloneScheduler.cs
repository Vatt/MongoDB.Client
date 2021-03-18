﻿using Microsoft.Extensions.Logging;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Settings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal class StandaloneScheduler : IMongoScheduler
    {
        private readonly MongoScheduler _mongoScheduler;

        public StandaloneScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _mongoScheduler = new MongoScheduler(settings, connectionFactory, loggerFactory);
        }


        public ValueTask StartAsync(CancellationToken token)
        {
            return _mongoScheduler.StartAsync(token);
        }

        public ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken token)
        {
            return _mongoScheduler.DropCollectionAsync(message, token);
        }

        public ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken token)
        {
            return _mongoScheduler.CreateCollectionAsync(message, token);
        }

        public async ValueTask<FindResult<T>> FindAsync<T>(BsonDocument filter, int limit, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token)
        {
            var requestNum = _mongoScheduler.GetNextRequestNumber();
            var requestDocument = new FindRequest(collectionNamespace.CollectionName, filter, limit, default, null, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new FindMessage(requestNum, requestDocument);
            var result = await _mongoScheduler.GetCursorAsync<T>(request, token).ConfigureAwait(false);
            return new FindResult<T>(result, _mongoScheduler);
        }


        public ValueTask<CursorResult<T>> GetMoreAsync<T>(MongoScheduler scheduler, long cursorId, CollectionNamespace collectionNamespace, TransactionHandler transaction, CancellationToken token)
        {
            var requestNum = scheduler.GetNextRequestNumber();
            var requestDocument = new FindRequest(null, null, default, cursorId, null, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new FindMessage(requestNum, requestDocument);
            return scheduler.GetCursorAsync<T>(request, token);
        }


        public ValueTask InsertAsync<T>(TransactionHandler transaction, IEnumerable<T> items, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var requestNumber = _mongoScheduler.GetNextRequestNumber();
            var insertHeader = new InsertHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new InsertMessage<T>(requestNumber, insertHeader, items);
            return _mongoScheduler.InsertAsync(request, token);
        }


        public ValueTask<DeleteResult> DeleteAsync(TransactionHandler transaction, BsonDocument filter, int limit, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var requestNumber = _mongoScheduler.GetNextRequestNumber();
            var deleteHeader = new DeleteHeader(collectionNamespace.CollectionName, true, collectionNamespace.DatabaseName, transaction.SessionId);

            var deleteBody = new DeleteBody(filter, limit);

            var request = new DeleteMessage(requestNumber, deleteHeader, deleteBody);
            return _mongoScheduler.DeleteAsync(request, token);
        }

        public ValueTask DropCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var requestNumber = _mongoScheduler.GetNextRequestNumber();
            var dropCollectionHeader = new DropCollectionHeader(collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new DropCollectionMessage(requestNumber, dropCollectionHeader);
            return _mongoScheduler.DropCollectionAsync(request, token);
        }

        public ValueTask CreateCollectionAsync(TransactionHandler transaction, CollectionNamespace collectionNamespace, CancellationToken token)
        {
            var requestNumber = _mongoScheduler.GetNextRequestNumber();
            var createCollectionHeader = new CreateCollectionHeader(collectionNamespace.CollectionName, collectionNamespace.DatabaseName, transaction.SessionId);
            var request = new CreateCollectionMessage(requestNumber, createCollectionHeader);
            return _mongoScheduler.CreateCollectionAsync(request, token);
        }

        public ValueTask CommitTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Transactions not support on standalone");
        }

        public ValueTask AbortTransactionAsync(TransactionHandler transactionHandler, CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Transactions not support on standalone");
        }

        public ValueTask DisposeAsync()
        {
            return _mongoScheduler.DisposeAsync();
        }
    }
}
