using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection
    {
        public int ConnectionId { get; }
        public int Threshold => 8;
        private System.Net.Connections.Connection _connection;
        private ILogger _logger;
        private readonly List<ParserCompletion> _completions;
        private ProtocolReader _protocolReader;
        private ProtocolWriter _protocolWriter;
        private readonly ChannelReader<ManualResetValueTaskSource<IParserResult>> _channelReader;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private Task? _listenerTask;

        internal MongoConnection(int connectionId, System.Net.Connections.Connection connection, ILogger logger, 
            ChannelReader<ManualResetValueTaskSource<IParserResult>> channelReader)
        {
            ConnectionId = connectionId;
            _connection = connection;
            _completions = new List<ParserCompletion>(Threshold);
            _logger = logger;
            _protocolReader = _connection.CreateReader();
            _protocolWriter = _connection.CreateWriter();
            _listenerTask = StartProtocolListenerAsync();
            _channelReader = channelReader;
        }
        

    }
}