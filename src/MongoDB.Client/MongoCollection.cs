﻿using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public class MongoCollection<T>
    {
        private readonly RequestScheduler _scheduler;

        internal MongoCollection(MongoDatabase database, string name, RequestScheduler scheduler)
        {
            _scheduler = scheduler;
            Database = database;
            Namespace = new CollectionNamespace(database.Name, name);
        }

        public MongoDatabase Database { get; }

        public CollectionNamespace Namespace { get; }

        public Cursor<T> Find(BsonDocument filter)
        {
            return new Cursor<T>(_scheduler, filter, Namespace);
        }

        public async ValueTask InsertAsync(T item, CancellationToken cancellationToken = default)
        {
            var list = ListsPool<T>.Pool.Get();
            try
            {
                list.Add(item);
                await InsertAsync(list, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ListsPool<T>.Pool.Return(list);
            }
        }

        public async ValueTask InsertAsync(IEnumerable<T> items, CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var insertHeader = new InsertHeader
            {
                Insert = Namespace.CollectionName,
                Ordered = true,
                Db = Namespace.DatabaseName,
                Lsid = SharedSessionIdModel
            };
            var request = new InsertMessage<T>(requestNumber, insertHeader, items);
            await _scheduler.InsertAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask<DeleteResult> DeleteOneAsync(BsonDocument filter, CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var deleteHeader = new DeleteHeader
            {
                Delete = Namespace.CollectionName,
                Ordered = true,
                Db = Namespace.DatabaseName,
                Lsid = SharedSessionIdModel
            };

            var deleteBody = new DeleteBody
            {
                Filter = filter,
                Limit = 1
            };

            var request = new DeleteMessage(requestNumber, deleteHeader, deleteBody);
            return await _scheduler.DeleteAsync(request, cancellationToken).ConfigureAwait(false);
        }

        internal async ValueTask DropAsync(CancellationToken cancellationToken)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var dropCollectionHeader = new DropCollectionHeader
            {
                CollectionName = Namespace.CollectionName,
                Db = Namespace.DatabaseName,
                Lsid = SharedSessionIdModel
            };
            var request = new DropCollectionMessage(requestNumber, dropCollectionHeader);
            await _scheduler.DropCollectionAsync(request, cancellationToken).ConfigureAwait(false);
        }

        internal async ValueTask CreateAsync(CancellationToken cancellationToken)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var createCollectionHeader = new CreateCollectionHeader
            {
                CollectionName = Namespace.CollectionName,
                Db = Namespace.DatabaseName,
                Lsid = SharedSessionIdModel
            };
            var request = new CreateCollectionMessage(requestNumber, createCollectionHeader);
            await _scheduler.CreateCollectionAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private static readonly SessionId SharedSessionIdModel = new SessionId { Id = Guid.NewGuid() };
    }
}