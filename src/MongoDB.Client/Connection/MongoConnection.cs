﻿using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
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
    }
}