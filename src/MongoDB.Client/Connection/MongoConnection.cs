using Microsoft.Extensions.Logging;
using MongoDB.Client.Exceptions;
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

        private ILogger _logger;
        private ConcurrentDictionary<long, MongoRequest> _completions;
        private ProtocolReader? _protocolReader;
        private ProtocolWriter? _protocolWriter;
        private readonly ChannelReader<MongoRequest> _channelReader;
        private readonly ChannelReader<MongoRequest> _findReader;
        private readonly RequestScheduler _requestScheduler;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _protocolListenerTask;
        private Task? _channelListenerTask;
        private Task? _channelFindListenerTask;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        private readonly MongoClientSettings _settings;
        internal MongoConnection(int connectionId, MongoClientSettings settings, ILogger logger, ChannelReader<MongoRequest> channelReader, ChannelReader<MongoRequest> findReader, RequestScheduler requestScheduler)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<long, MongoRequest>();
            _logger = logger;
            _channelReader = channelReader;
            _findReader = findReader;
            _requestScheduler = requestScheduler;
            _settings = settings;
        }

        public async ValueTask DisposeAsync()
        {
            _shutdownCts.Cancel();
            if (_channelFindListenerTask is not null)
            {
                await _channelFindListenerTask.ConfigureAwait(false);
            }
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