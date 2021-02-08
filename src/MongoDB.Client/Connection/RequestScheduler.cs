using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{

    internal partial class RequestScheduler //: IAsyncDisposable
    {
        private readonly IMongoConnectionFactory _connectionFactory;
        private ImmutableArray<MongoConnection> _connections = ImmutableArray<MongoConnection>.Empty;
        private readonly MongoClientSettings _settings;
        private static int _counter;
        private int _connectionsCount;
        private int _channelNumber;
        private static readonly Random Random = new();
        private readonly SemaphoreSlim _channelAllocateLock = new(1);
        public RequestScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _settings = settings;
            _counter = 10;
        }
        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }
        internal ValueTask InitAsync()
        {
            return default;
        }
        private ValueTask<MongoConnection> GetConnection()
        {
            var idx = Interlocked.Increment(ref _connectionsCount);
            var connections = _connections;

            for (int i = 0; i < connections.Length; i++)
            {
                var current = (idx + i) % connections.Length;
                var connection = connections[current];
                if (connection.RequestsInProgress < 2)
                {
                    return new ValueTask<MongoConnection>(connection);
                }
            }

            if (connections.Length == _settings.ConnectionPoolMaxSize)
            {
                idx = Random.Next(_settings.ConnectionPoolMaxSize);
                return new ValueTask<MongoConnection>(connections[idx]);
            }
            return CreateNewConnection(default);

        }

        private async ValueTask<MongoConnection> CreateNewConnection(CancellationToken cancellationToken)
        {
            await _channelAllocateLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                MongoConnection channel;
                var channels = _connections;
                for (int i = 0; i < channels.Length; i++)
                {
                    channel = channels[i];
                    if (channel.RequestsInProgress < 2)
                    {
                        return channel;
                    }
                }

                if (channels.Length == _settings.ConnectionPoolMaxSize)
                {
                    var idx = Random.Next(_settings.ConnectionPoolMaxSize);
                    return channels[idx];
                }

                channel = await CreateChannelAsync(cancellationToken);


                _connections = channels.Add(channel);
                return channel;
            }
            finally
            {
                _channelAllocateLock.Release();
            }
        }

        private async Task<MongoConnection> CreateChannelAsync(CancellationToken token)
        {
            var channelNum = Interlocked.Increment(ref _channelNumber);
            var channel = await _connectionFactory.CreateAsync(_settings);
            return channel;
        }
        internal async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token = default)
        {
            var connection = await GetConnection();
            var result = await connection.GetCursorAsync<T>(message, token);
            return result;
        }

        internal async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token = default)
        {
            var connection = await GetConnection();
            await connection.InsertAsync(message, token);
        }
        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken token)
        {
            var connection = await GetConnection();
            var result = await connection.DeleteAsync(message, token);
            return result;
        }

        public async ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken token)
        {
            var connection = await GetConnection();
            await connection.DropCollectionAsync(message, token);
        }

        public async ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken token)
        {
            var connection = await GetConnection();
            await connection.CreateCollectionAsync(message, token);
        }

        //public async ValueTask DisposeAsync()
        //{
        //    _channelWriter.Complete();
        //    foreach (var connection in _connections)
        //    {
        //        await connection.DisposeAsync().ConfigureAwait(false);
        //    }
        //}
    }
}
