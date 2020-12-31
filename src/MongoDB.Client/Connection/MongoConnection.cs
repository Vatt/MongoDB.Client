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
        public int Threshold => 8;
        private ConnectionContext _connection;
        private ILogger _logger;
        private ConcurrentDictionary<long, MongoReuqestBase> _completions;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private readonly ChannelReader<MongoReuqestBase> _channelReader;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _protocolListenerTask;
        private Task? _channelListenerTask;
        private readonly ConcurrentQueue<ManualResetValueTaskSource<IParserResult>> _queue = new();
        internal MongoConnection(int connectionId, ILogger logger, ChannelReader<MongoReuqestBase> channelReader)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<long, MongoReuqestBase>();
            _logger = logger;
            _channelReader = channelReader;
        }

        public async ValueTask DisposeAsync()
        {
            _shutdownCts.Cancel();
            if (_channelListenerTask is not null)
            {
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