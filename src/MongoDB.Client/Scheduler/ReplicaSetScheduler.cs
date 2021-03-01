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
    internal class ReplicaSetScheduler : IMongoScheduler
    {
        private List<StandaloneScheduler> _shedulers;
        private ILogger _logger;
        private int _schedulerCounter = 0;
        private int _requestCounter = 0;
        public ReplicaSetScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ReplicaSetScheduler>();
            var endpoints = settings.Endpoints;
            var maxConnections = settings.ConnectionPoolMaxSize / endpoints.Length;
            _shedulers = new();
            for (var i = 0; i < endpoints.Length; i++)
            {
                var scheduler = new StandaloneScheduler(maxConnections, settings, new MongoConnectionFactory(endpoints[i], loggerFactory), loggerFactory);
                _shedulers.Add(scheduler);
            }
        }
        private StandaloneScheduler GetScheduler()
        {
            Interlocked.Increment(ref _schedulerCounter);
            //var schedulerId = _schedulerCounter % _shedulers.Count;
            var schedulerId = 0;
            var result = _shedulers[schedulerId];
            return result;
        }
        public async ValueTask InitAsync()
        {
            foreach (var scheduler in _shedulers)
            {
                await scheduler.InitAsync();
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
            return GetScheduler().CreateCollectionAsync(message, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            return GetScheduler().DeleteAsync(message, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken cancellationToken)
        {
            return GetScheduler().DropCollectionAsync(message, cancellationToken);
        }

        public ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token)
        {
            return GetScheduler().GetCursorAsync<T>(message, token);
        }

        public ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token)
        {
            return GetScheduler().InsertAsync(message, token);
        }
    }
}
