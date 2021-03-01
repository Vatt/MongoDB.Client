using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Scheduler
{
    internal sealed class ReplicaSetScheduler : IMongoScheduler
    {
        private List<IReplicaSetScheduler> _shedulers;
        private IReplicaSetScheduler _master;
        private List<IReplicaSetScheduler> _slaves;
        private ILogger _logger;
        private int _schedulerCounter = 0;
        private int _requestCounter = 0;
        public ReplicaSetScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReplicaSetScheduler>();
            var endpoints = settings.Endpoints;
            var maxConnections = settings.ConnectionPoolMaxSize / endpoints.Length;
            _shedulers = new();
            _slaves = new();
            for (var i = 0; i < endpoints.Length; i++)
            {
                var scheduler = new ReplicaSetInnerScheduler(maxConnections, settings, new MongoConnectionFactory(endpoints[i], loggerFactory), loggerFactory);
                _shedulers.Add(scheduler);
            }
        }
        private IReplicaSetScheduler GetSlaveScheduler()
        {
            Interlocked.Increment(ref _schedulerCounter);
            var schedulerId = _schedulerCounter % _slaves.Count;
            var result = _slaves[schedulerId];
            return result;
        }
        public async ValueTask InitAsync()
        {
            foreach (var scheduler in _shedulers)
            {
                await scheduler.InitAsync();
                if (scheduler.IsMaster)
                {
                    _master = scheduler;
                }
                else
                {
                    _slaves.Add(scheduler);
                }
            }
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _requestCounter);
        }

        public Task ConnectionLost(MongoConnection connection)
        {
            throw new NotImplementedException();
        }

        public ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken cancellationToken)
        {
            return GetSlaveScheduler().CreateCollectionAsync(message, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            return GetSlaveScheduler().DeleteAsync(message, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken)
        {
            return _master.DropCollectionAsync(message, cancellationToken);
        }

        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token)
        {
            var result = await GetSlaveScheduler().GetCursorAsync<T>(message, token);
            return result;
        }

        public ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token)
        {
            return _master.InsertAsync(message, token);
        }
    }
}
