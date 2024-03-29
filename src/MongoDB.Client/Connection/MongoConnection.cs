﻿using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection : IAsyncDisposable
    {
        public int ConnectionId { get; }
        private ILogger _logger;
        private ConcurrentDictionary<long, MongoRequest> _completions;
        private ProtocolReader? _protocolReader;
        private ProtocolWriter? _protocolWriter;
        private readonly ChannelReader<MongoRequest> _channelReader;
        //private readonly ChannelReader<MongoRequest> _findReader;
        private readonly MongoScheduler _requestScheduler;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _protocolListenerTask;
        private Task? _channelListenerTask;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        private readonly MongoClientSettings _settings;

        internal MongoConnection(int connectionId, MongoClientSettings settings, ILogger logger, ChannelReader<MongoRequest> channelReader, MongoScheduler requestScheduler)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<long, MongoRequest>();
            _logger = logger;
            _channelReader = channelReader;
            _requestScheduler = requestScheduler;
            _settings = settings;
        }
        public async ValueTask DisposeAsync()
        {
            _shutdownCts.Cancel();
            if (_channelListenerTask is not null)
            {
                await _channelListenerTask.ConfigureAwait(false);
            }
            if (_protocolWriter is not null)
            {
                await _protocolWriter.DisposeAsync().ConfigureAwait(false);
            }
            if (_protocolListenerTask is not null)
            {
                await _protocolListenerTask.ConfigureAwait(false);
            }
            if (_protocolReader is not null)
            {
                await _protocolReader.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
