using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection : IAsyncDisposable
    {
        public int ConnectionId { get; }
        public int Threshold => 4;
        private ConnectionContext _connection;
        private ILogger _logger;
        private ConcurrentDictionary<long, MongoRequestBase> _completions;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private readonly ChannelReader<MongoRequestBase> _channelReader;
        private readonly ChannelReader<FindMongoRequest> _findReader;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _protocolListenerTask;
        private Task? _channelListenerTask;
        private Task? _channelFindListenerTask;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        private readonly SemaphoreSlim _channelListenerLock = new(0);
        private readonly MongoClientSettings _settings;
        internal MongoConnection(int connectionId, MongoClientSettings settings, ILogger logger, ChannelReader<MongoRequestBase> channelReader, ChannelReader<FindMongoRequest> findReader)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<long, MongoRequestBase>();
            _logger = logger;
            _channelReader = channelReader;
            _findReader = findReader;
            _settings = settings;
        }

        public async ValueTask DisposeAsync()
        {
            _shutdownCts.Cancel();
            if (_channelListenerTask is not null)
            {
                await _channelFindListenerTask.ConfigureAwait(false);
                await _channelListenerTask.ConfigureAwait(false);
                await _protocolWriter.DisposeAsync().ConfigureAwait(false);
            }
            if (_protocolListenerTask is not null)
            {
                await _protocolListenerTask.ConfigureAwait(false);
                await _protocolReader.DisposeAsync().ConfigureAwait(false);
            }
            if (_connection is not null)
            {
                //TODO: CHECK IT!
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}