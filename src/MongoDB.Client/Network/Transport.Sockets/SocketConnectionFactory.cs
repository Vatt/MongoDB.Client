using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Client.Network.Transport.Sockets.Internal;

namespace MongoDB.Client.Network.Transport.Sockets
{
    internal class SocketConnectionFactory : IConnectionFactory, IAsyncDisposable
    {
        private readonly SocketTransportOptions _options;
        private readonly MemoryPool<byte> _memoryPool;
        private readonly SocketsTrace _trace;

        public SocketConnectionFactory(IOptions<SocketTransportOptions> options, ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _options = options.Value;
            _memoryPool = options.Value.MemoryPoolFactory();
            var logger = loggerFactory.CreateLogger("MongoDB.Client.Network.Transport.Sockets");
            _trace = new SocketsTrace(logger);
        }
        public async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = _options.NoDelay
            };

            await socket.ConnectAsync(endpoint);

            var socketConnection = new SocketConnection(
                socket,
                _memoryPool,
                PipeScheduler.ThreadPool,
                _trace,
                _options.MaxReadBufferSize,
                _options.MaxWriteBufferSize,
                _options.WaitForDataBeforeAllocatingBuffer,
                _options.UnsafePreferInlineScheduling);

            socketConnection.Start();
            return socketConnection;
        }

        public ValueTask DisposeAsync()
        {
            _memoryPool.Dispose();
            return default;
        }
    }
}
