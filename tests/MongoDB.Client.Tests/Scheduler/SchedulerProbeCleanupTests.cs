using System.IO.Pipelines;
using System.Collections.Immutable;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Network.Transport.Abstractions;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Scheduler
{
    public class SchedulerProbeCleanupTests
    {
        [Fact]
        public async Task ReplicaSetStartAsync_WhenProbeConnectFails_DisposesTemporaryConnectionContext()
        {
            var context = new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27017));
            var scheduler = new ReplicaSetScheduler(
                CreateSettings(context.RemoteEndPoint!),
                NullLoggerFactory.Instance,
                new ThrowingInitializer(),
                (endpoint, cancellationToken) => ValueTask.FromResult<Microsoft.AspNetCore.Connections.ConnectionContext>(context));

            await Assert.ThrowsAnyAsync<Exception>(async () => await scheduler.StartAsync(CancellationToken.None));

            Assert.True(context.IsDisposed);
        }

        [Fact]
        public async Task ShardedStartAsync_WhenProbeConnectFails_DisposesTemporaryConnectionContext()
        {
            var context = new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27017));
            var scheduler = new ShardedScheduler(
                CreateSettings(context.RemoteEndPoint!),
                NullLoggerFactory.Instance,
                new ThrowingInitializer(),
                (endpoint, cancellationToken) => ValueTask.FromResult<Microsoft.AspNetCore.Connections.ConnectionContext>(context));

            await scheduler.StartAsync(CancellationToken.None);

            Assert.True(context.IsDisposed);
            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task StartAsync_WhenPoolWarmupFails_DisposesAlreadyCreatedConnections()
        {
            var contexts = new[]
            {
                new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27017)),
                new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27018)),
                new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27019))
            };
            var nextContext = 0;
            var factory = new MongoConnectionFactory(
                contexts[0].RemoteEndPoint!,
                NullLoggerFactory.Instance,
                (endpoint, cancellationToken) => ValueTask.FromResult<Microsoft.AspNetCore.Connections.ConnectionContext>(contexts[nextContext++]));
            var scheduler = new MongoScheduler(
                new MongoClientSettings { ConnectionPoolMaxSize = contexts.Length },
                factory,
                NullLoggerFactory.Instance,
                new FailOnThirdInitializer());

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await scheduler.StartAsync(CancellationToken.None));

            Assert.All(contexts, context => Assert.True(context.IsDisposed));
            Assert.Equal(0, GetConnectionCount(scheduler));
            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task StandaloneStartAsync_WhenInitializerFails_DisposesConnectionContext()
        {
            var context = new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27017));
            var factory = new MongoConnectionFactory(
                context.RemoteEndPoint!,
                NullLoggerFactory.Instance,
                (endpoint, cancellationToken) => ValueTask.FromResult<Microsoft.AspNetCore.Connections.ConnectionContext>(context));
            var scheduler = new StandaloneScheduler(
                CreateSettings(context.RemoteEndPoint!),
                factory,
                NullLoggerFactory.Instance,
                new ThrowingInitializer());

            await Assert.ThrowsAnyAsync<Exception>(async () => await scheduler.StartAsync(CancellationToken.None));

            Assert.True(context.IsDisposed);
        }

        private static MongoClientSettings CreateSettings(EndPoint endPoint)
        {
            return new MongoClientSettings
            {
                Endpoints = ImmutableArray.Create(endPoint),
                ConnectionPoolMaxSize = 1
            };
        }

        private sealed class ThrowingInitializer : IMongoConnectionInitializer
        {
            public ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("initializer failed");
            }
        }

        private sealed class FailOnThirdInitializer : IMongoConnectionInitializer
        {
            private int _calls;

            public ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
            {
                if (Interlocked.Increment(ref _calls) == 3)
                {
                    throw new InvalidOperationException("initializer failed");
                }

                return ValueTask.FromResult(new ConnectionInfo(new MongoDB.Client.Bson.Document.BsonDocument(), new MongoDB.Client.Bson.Document.BsonDocument()));
            }
        }

        private sealed class TrackingConnectionContext : TransportConnection
        {
            private readonly CancellationTokenSource _connectionClosedSource = new();

            public TrackingConnectionContext(EndPoint remoteEndPoint)
            {
                var pair = DuplexPipe.CreateConnectionPair(new PipeOptions(), new PipeOptions());
                Transport = pair.Transport;
                Application = pair.Application;
                RemoteEndPoint = remoteEndPoint;
                ConnectionClosed = _connectionClosedSource.Token;
            }

            public bool IsDisposed { get; private set; }

            public override void Abort(Microsoft.AspNetCore.Connections.ConnectionAbortedException abortReason)
            {
                _connectionClosedSource.Cancel();
                base.Abort(abortReason);
            }

            public override ValueTask DisposeAsync()
            {
                IsDisposed = true;
                _connectionClosedSource.Cancel();
                return ValueTask.CompletedTask;
            }
        }

        private static int GetConnectionCount(MongoScheduler scheduler)
        {
            var field = typeof(MongoScheduler).GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return Assert.IsType<List<MongoConnection>>(field!.GetValue(scheduler)).Count;
        }
    }
}
