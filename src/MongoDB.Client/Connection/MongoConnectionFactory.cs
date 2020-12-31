﻿using Microsoft.Extensions.Logging;
using MongoDB.Client.Network;
using MongoDB.Client.Network.Transport.Sockets.Internal;
using System;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal class MongoConnectionFactory
    {
        private static int CONNECTION_ID = 0;
        private readonly NetworkConnectionFactory _networkFactory;
        private readonly EndPoint _endPoint;
        private readonly ILoggerFactory _loggerFactory;
        public MongoConnectionFactory(EndPoint endPoint, ILoggerFactory loggerFactory)
        {
            _networkFactory = new NetworkConnectionFactory(loggerFactory);
            _endPoint = endPoint;
            _loggerFactory = loggerFactory;
        }

        public async ValueTask<MongoConnection> Create(ChannelReader<MongoReuqestBase> reader)
        {
            var context = await _networkFactory.ConnectAsync(_endPoint);
            if (context is null)
            {
                ThrowHelper.ConnectionException<SocketConnection>(_endPoint);
            }
            var id = Interlocked.Increment(ref CONNECTION_ID);
            var connection = new MongoConnection(id, _loggerFactory.CreateLogger(String.Empty), reader);
            await connection.StartAsync(context);
            return connection;
        }
    }
}