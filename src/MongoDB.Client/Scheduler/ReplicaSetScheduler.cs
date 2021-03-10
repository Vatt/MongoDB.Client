using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Network;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ReadPreference = MongoDB.Client.Settings.ReadPreference;

namespace MongoDB.Client.Scheduler
{
    internal sealed class ReplicaSetScheduler : IMongoScheduler
    {
        private readonly NetworkConnectionFactory _networkfactory;
        private readonly ILoggerFactory _loggerFactory;
        private MongoServiceConnection _service;
        private MongoClientSettings _settings;
        private List<IMongoScheduler> _shedulers;
        private IMongoScheduler _master;
        private List<IMongoScheduler> _slaves;
        private ILogger _logger;
        private MongoPingMessage _lastPing;
        private int _schedulerCounter = 0;
        private int _requestCounter = 0;

        public MongoClusterTime ClusterTime => _lastPing?.ClusterTime!;

        public ReplicaSetScheduler(MongoClientSettings settings, ILoggerFactory loggerFactory)
        {
            _settings = settings;
            _logger = loggerFactory.CreateLogger<ReplicaSetScheduler>();
            _networkfactory = new NetworkConnectionFactory(loggerFactory);
            _loggerFactory = loggerFactory;
            _shedulers = new();
            _slaves = new();
        }


        private IMongoScheduler GetSlaveScheduler()
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
            for (int i = 0; i < _settings.Endpoints.Length; i++)
            {
                ConnectionContext? ctx;
                try
                {
                    ctx = await _networkfactory.ConnectAsync(_settings.Endpoints[i]);
                }
                catch (Exception ex)
                {
                    continue;
                }
                _service = new MongoServiceConnection(ctx);
                break;
            }
            if (_service is null)
            {
                ThrowHelper.MongoInitExceptions();
            }
            await _service.Connect(_settings, default).ConfigureAwait(false);
            _lastPing = await _service.MongoPing().ConfigureAwait(false);
            if (_lastPing.Primary is null)
            {
                ThrowHelper.PrimaryNullExceptions(); //TODO: fixit
            }
            for (var i = 0; i < _lastPing.Hosts.Count; i++)
            {
                var host = _lastPing.Hosts[i];
                var scheduler = new StandaloneScheduler(maxConnections, _settings, new MongoConnectionFactory(host, _loggerFactory), _loggerFactory);
                _shedulers.Add(scheduler);
                if (host.Equals(_lastPing.Primary))
                {
                    _master = scheduler;
                }
                else
                {
                    _slaves.Add(scheduler);
                }
                await scheduler.StartAsync();
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
            return _master.CreateCollectionAsync(message, cancellationToken);
        }

        public ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken cancellationToken)
        {
            return _master.DeleteAsync(message, cancellationToken);
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
            var scheduler = GetScheduler();
            message.Document.ReadPreference = new Messages.ReadPreference(_settings.ReadPreference);
            message.Document.ClusterTime = _lastPing.ClusterTime;
            var result = await scheduler.GetCursorAsync<T>(message, token).ConfigureAwait(false);
            return result;
        }

        private IMongoScheduler GetScheduler()
        {
            IMongoScheduler? scheduler = null;
            switch (_settings.ReadPreference)
            {
                case ReadPreference.Primary:
                    scheduler = _master;
                    break;
                case ReadPreference.PrimaryPreferred:
                    scheduler = _master;
                    if (scheduler is null)
                    {
                        scheduler = GetSlaveScheduler();
                    }
                    break;
                case ReadPreference.Secondary:
                    scheduler = GetSlaveScheduler();
                    break;
                case ReadPreference.SecondaryPreferred:
                    scheduler = GetSlaveScheduler();
                    if (scheduler is null)
                    {
                        scheduler = _master;
                    }
                    break;
                case ReadPreference.Nearest:
                default:
                    ReadPreferenceNotSupported(_settings.ReadPreference);
                    break;
            }
            if (scheduler is null)
            {
                ThrowSchedulerNotFound();
            }
            return scheduler;
        }

        public ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token)
        {
            return _master.InsertAsync(message, token);
        }


        // TODO:
        [DoesNotReturn]
        private static void ThrowSchedulerNotFound()
        {
            throw new Exception("Scheduler Not Found");
        }

        // TODO:
        [DoesNotReturn]
        private static void ReadPreferenceNotSupported(ReadPreference readPreference)
        {
            throw new Exception(readPreference.ToString() + " is not supported");
        }
    }
}
