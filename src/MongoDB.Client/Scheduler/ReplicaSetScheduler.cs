using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
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
        private readonly NetworkConnectionFactory _factory;
        private MongoServiceConnection _service;
        private MongoClientSettings _settings;
        private List<IReplicaSetNodeScheduler> _shedulers;
        private IReplicaSetNodeScheduler _master;
        private List<IReplicaSetNodeScheduler> _slaves;
        private ILogger _logger;
        private int _schedulerCounter = 0;
        private int _requestCounter = 0;

        public ReplicaSetScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _logger = loggerFactory.CreateLogger<ReplicaSetScheduler>();
            _factory = new NetworkConnectionFactory(loggerFactory);
            _shedulers = new();
            _slaves = new();
        }
        private IReplicaSetNodeScheduler GetSlaveScheduler()
        {
            Interlocked.Increment(ref _schedulerCounter);
            var schedulerId = _schedulerCounter % _slaves.Count;
            var result = _slaves[schedulerId];
            return result;
        }
        public async ValueTask StartAsync()
        {
            var endpoints = _settings.Endpoints;
            var maxConnections = _settings.ConnectionPoolMaxSize / endpoints.Length;
            _service = new MongoServiceConnection(await _factory.ConnectAsync(_settings.Endpoints[0])); //TODO: fix it
            await _service.Connect(_settings, default).ConfigureAwait(false);
            var pingDoc = await _service.MongoPing().ConfigureAwait(false);
            //for (var i = 0; i < endpoints.Length; i++)
            //{
            //    var scheduler = new ReplicaSetNodeScheduler(maxConnections, settings, new MongoConnectionFactory(endpoints[i], loggerFactory), loggerFactory);
            //    _shedulers.Add(scheduler);
            //}
            //foreach (var scheduler in _shedulers)
            //{
            //    await scheduler.InitAsync();
            //    if (await scheduler.IsMaster())
            //    {
            //        _master = scheduler;
            //    }
            //    else
            //    {
            //        _slaves.Add(scheduler);
            //    }
            //}
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
