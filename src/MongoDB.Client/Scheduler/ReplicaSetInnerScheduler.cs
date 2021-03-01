﻿using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal class ReplicaSetInnerScheduler : IReplicaSetScheduler
    {
        private StandaloneScheduler _inner;
        public bool IsMaster 
        {
            get 
            {
                return _inner._connections[0].IsMaster;
            }
        }
        public ReplicaSetInnerScheduler(int maxConnections, MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory)
        {
            _inner = new StandaloneScheduler(maxConnections, settings, connectionFactory, loggerFactory);
        }
        public Task ConnectionLost(MongoConnection connection)
        {
            throw new NotImplementedException();
        }

        public ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken cancellationToken)
        {
            return _inner.CreateCollectionAsync(message, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            return _inner.DeleteAsync(message, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return _inner.DisposeAsync();
        }

        public ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken)
        {
            return _inner.DropCollectionAsync(message, cancellationToken);
        }

        public ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token)
        {
            return _inner.GetCursorAsync<T>(message, token);
        }

        public int GetNextRequestNumber()
        {
            return _inner.GetNextRequestNumber();
        }

        public async ValueTask InitAsync()
        {
            await _inner.InitAsync();
        }

        public ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token)
        {
            return _inner.InsertAsync(message, token);
        }
    }
}
