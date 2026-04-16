using System.Buffers;
using System.Reflection;
using System.IO.Pipelines;
using System.Threading.Channels;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Client
{
    public class MongoSchedulerReconnectTests
    {
        [Fact]
        public async Task ConnectionLost_ReplacesLostConnectionUsingFactory()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var scheduler = new MongoScheduler(
                settings,
                new RecordingMongoConnectionFactory(),
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var initialConnection = Assert.Single(GetConnections(scheduler));
            var factory = Assert.IsType<RecordingMongoConnectionFactory>(GetConnectionFactory(scheduler));

            await scheduler.ConnectionLost(initialConnection);

            var replacementConnection = Assert.Single(GetConnections(scheduler));
            Assert.NotSame(initialConnection, replacementConnection);
            Assert.Same(factory.CreatedConnections[1], replacementConnection);
            Assert.Equal(2, factory.CreateCalls);
            Assert.All(factory.RequestSchedulers, requestScheduler => Assert.Same(scheduler, requestScheduler));

            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task ConnectionLost_DuringWarmup_DoesNotPublishDeadConnection()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 2
            };
            var factory = new WarmupDisconnectMongoConnectionFactory(blockOnCallNumber: 2);
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            var startTask = scheduler.StartAsync(CancellationToken.None).AsTask();
            await factory.WaitUntilBlockedAsync();

            await factory.Transports[0].DisconnectAsync();
            await WaitUntilAsync(() => IsFaultedOrDisposed(factory.CreatedConnections[0]));

            factory.ReleaseBlockedCall();
            await startTask.WaitAsync(TimeSpan.FromSeconds(5));

            var liveConnections = GetConnections(scheduler);
            Assert.Equal(3, factory.CreateCalls);
            Assert.Equal(2, liveConnections.Count);
            Assert.DoesNotContain(factory.CreatedConnections[0], liveConnections);
            Assert.All(liveConnections, connection => Assert.False(IsFaultedOrDisposed(connection)));

            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task DisposeAsync_WhileReconnectInFlight_DoesNotPublishReplacementConnection()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new BlockingMongoConnectionFactory(blockOnCallNumber: 2);
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var reconnectTask = scheduler.ConnectionLost(factory.CreatedConnections[0]);
            await factory.WaitUntilBlockedAsync();

            var disposeTask = scheduler.DisposeAsync().AsTask();
            await Task.WhenAll(reconnectTask, disposeTask).WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal(2, factory.CreateCalls);
            Assert.True(factory.CreateCanceled);
            Assert.Empty(GetConnections(scheduler));
        }

        [Fact]
        public async Task DisposeAsync_DuringWarmupInitialization_CancelsInitializerViaShutdownToken()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var initializer = new BlockingInitializer();
            var scheduler = new MongoScheduler(
                settings,
                new InitializerAwareMongoConnectionFactory(),
                NullLoggerFactory.Instance,
                initializer);

            var startTask = scheduler.StartAsync(CancellationToken.None).AsTask();
            await initializer.WaitUntilEnteredAsync();

            await scheduler.DisposeAsync();

            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => startTask);
            Assert.Equal(nameof(MongoScheduler), exception.ObjectName);
            Assert.True(initializer.CancellationObserved);
            Assert.Empty(GetConnections(scheduler));
        }

        [Fact]
        public async Task StartAsync_WhenWarmupConnectionsDisconnectImmediately_FailsAfterBoundedAttempts()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new AlwaysDisconnectingWarmupConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            var exception = await Assert.ThrowsAsync<MongoDB.Client.Exceptions.MongoException>(async () =>
                await scheduler.StartAsync(CancellationToken.None).AsTask().WaitAsync(TimeSpan.FromSeconds(5)));

            Assert.Equal("Warmup failed after 2 connection attempts.", exception.Message);
            Assert.Equal(2, factory.CreateCalls);
            Assert.Empty(GetConnections(scheduler));
        }

        [Fact]
        public async Task StartAsync_WhenDisposeBeginsBeforeWarmupPublish_DoesNotTransitionBackToStarted()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new DisposeDuringWarmupPublishMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            factory.AttachScheduler(scheduler);

            var startTask = scheduler.StartAsync(CancellationToken.None).AsTask();
            await factory.WaitUntilDisposeStartedAsync();

            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => startTask);
            Assert.Equal(nameof(MongoScheduler), exception.ObjectName);

            await factory.DisposeTask!.WaitAsync(TimeSpan.FromSeconds(5));

            Assert.Equal(SchedulerLifecycleState.Disposed, GetLifecycleState(scheduler));
            Assert.Empty(GetConnections(scheduler));
        }

        [Fact]
        public async Task DisposeAsync_WhenConnectionDisposeThrows_FinalizesDisposedStateAndDisposesRemainingConnections()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 2
            };
            var factory = new DisposeTrackingMongoConnectionFactory(throwOnDisposeConnectionId: 1);
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => scheduler.DisposeAsync().AsTask());

            Assert.Equal("cleanup failed", exception.Message);
            Assert.Equal(SchedulerLifecycleState.Disposed, GetLifecycleState(scheduler));
            Assert.Empty(GetConnections(scheduler));
            Assert.All(factory.Owners, owner => Assert.Equal(1, owner.DisposeCount));
        }

        [Fact]
        public async Task ChannelWriteFault_ReplacesConnectionAndFailsPendingRequest()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new WriteFaultMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var initialConnection = Assert.Single(GetConnections(scheduler));
            var request = new MongoRequest(new ManualResetValueTaskSource<IParserResult>())
            {
                RequestNumber = 1,
                WriteAsync = static (protocol, token) => protocol.WriteAsync(NoopMessageWriter.Instance, new object(), token),
                ParseAsync = static (_, _) => ValueTask.FromResult<IParserResult>(null!)
            };

            await WriteRequestAsync(scheduler, request);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.CompletionSource.GetValueTask().AsTask());
            Assert.Equal("write failed", exception.Message);

            await WaitUntilAsync(() =>
            {
                var current = GetConnections(scheduler);
                return current.Count == 1 && !ReferenceEquals(current[0], initialConnection);
            });

            Assert.Equal(2, factory.CreateCalls);
            Assert.Single(GetConnections(scheduler));
            Assert.DoesNotContain(initialConnection, GetConnections(scheduler));

            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task ConnectionLost_WhenReplacementCreationFailsAndPoolBecomesEmpty_TransitionsToFailedAndFailsBufferedRequests()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new FailOnReconnectMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var bufferedRequest = CreateBufferedRequest(101);
            await WriteRequestAsync(scheduler, bufferedRequest);

            await scheduler.ConnectionLost(factory.CreatedConnections[0]);

            var exception = await Assert.ThrowsAsync<MongoDB.Client.Exceptions.MongoException>(() => bufferedRequest.CompletionSource.GetValueTask().AsTask());
            Assert.Equal("Scheduler lost its last connection and failed to reconnect.", exception.Message);
            Assert.Equal(SchedulerLifecycleState.Failed, GetLifecycleState(scheduler));
            Assert.Empty(GetConnections(scheduler));
            Assert.Equal(2, factory.CreateCalls);

            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task ConnectionLost_WhenAnotherReconnectIsStillInFlight_DoesNotTransitionToFailed()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 2
            };
            var factory = new MixedReconnectOutcomeMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var initialConnections = GetConnections(scheduler).ToArray();
            var reconnect1 = scheduler.ConnectionLost(initialConnections[0]);
            var reconnect2 = scheduler.ConnectionLost(initialConnections[1]);

            await factory.WaitUntilRecoveringReconnectIsBlockedAsync();

            Assert.Equal(SchedulerLifecycleState.Started, GetLifecycleState(scheduler));
            Assert.Empty(GetConnections(scheduler));

            factory.ReleaseRecoveringReconnect();
            await Task.WhenAll(reconnect1, reconnect2).WaitAsync(TimeSpan.FromSeconds(5));

            var liveConnections = GetConnections(scheduler);
            Assert.Equal(SchedulerLifecycleState.Started, GetLifecycleState(scheduler));
            Assert.Single(liveConnections);
            Assert.Equal(4, factory.CreateCalls);
            Assert.All(liveConnections, connection => Assert.False(IsFaultedOrDisposed(connection)));

            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task ConnectionLost_WhenReplacementFaultsBeforePublish_DoesNotPublishDeadConnection()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new FaultedReplacementMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            await scheduler.ConnectionLost(factory.CreatedConnections[0]);

            Assert.Equal(SchedulerLifecycleState.Failed, GetLifecycleState(scheduler));
            Assert.Empty(GetConnections(scheduler));
            Assert.Equal(2, factory.CreateCalls);
            Assert.True(IsFaultedOrDisposed(factory.CreatedConnections[1]));

            await scheduler.DisposeAsync();
        }

        [Fact]
        public async Task DisposeAsync_FailsBufferedRequestsStillInSchedulerChannel()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new RecordingMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);

            var bufferedRequest = CreateBufferedRequest(202);
            await WriteRequestAsync(scheduler, bufferedRequest);

            await scheduler.DisposeAsync();

            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => bufferedRequest.CompletionSource.GetValueTask().AsTask());
            Assert.Equal(nameof(MongoScheduler), exception.ObjectName);
            Assert.Empty(GetConnections(scheduler));
        }

        [Fact]
        public async Task ConnectionLost_AfterDisposeAsync_CompletesQuietly()
        {
            var settings = new MongoClientSettings
            {
                ConnectionPoolMaxSize = 1
            };
            var factory = new RecordingMongoConnectionFactory();
            var scheduler = new MongoScheduler(
                settings,
                factory,
                NullLoggerFactory.Instance,
                new NoopConnectionInitializer());

            await scheduler.StartAsync(CancellationToken.None);
            var connection = factory.CreatedConnections[0];

            await scheduler.DisposeAsync();

            await scheduler.ConnectionLost(connection).WaitAsync(TimeSpan.FromSeconds(5));
        }

        private static IMongoConnectionFactory GetConnectionFactory(MongoScheduler scheduler)
        {
            var field = typeof(MongoScheduler).GetField("_connectionFactory", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return Assert.IsAssignableFrom<IMongoConnectionFactory>(field!.GetValue(scheduler));
        }

        private static List<MongoConnection> GetConnections(MongoScheduler scheduler)
        {
            var field = typeof(MongoScheduler).GetField("_connections", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return Assert.IsType<List<MongoConnection>>(field!.GetValue(scheduler));
        }

        private static SchedulerLifecycleState GetLifecycleState(MongoScheduler scheduler)
        {
            var field = typeof(MongoScheduler).GetField("_lifecycleState", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (SchedulerLifecycleState)Assert.IsType<int>(field!.GetValue(scheduler));
        }

        private static bool IsFaultedOrDisposed(MongoConnection connection)
        {
            var property = typeof(MongoConnection).GetProperty("IsFaultedOrDisposed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(property);
            return Assert.IsType<bool>(property!.GetValue(connection));
        }

        private static async Task WriteRequestAsync(MongoScheduler scheduler, MongoRequest request)
        {
            var field = typeof(MongoScheduler).GetField("_channelWriter", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            var writer = Assert.IsAssignableFrom<ChannelWriter<MongoRequest>>(field!.GetValue(scheduler));
            await writer.WriteAsync(request);
        }

        private static MongoRequest CreateBufferedRequest(int requestNumber)
        {
            return new MongoRequest(new ManualResetValueTaskSource<IParserResult>())
            {
                RequestNumber = requestNumber,
                WriteAsync = static (protocol, token) => protocol.WriteAsync(NoopMessageWriter.Instance, new object(), token),
                ParseAsync = static (_, _) => ValueTask.FromResult<IParserResult>(null!)
            };
        }

        private static async Task WaitUntilAsync(Func<bool> condition)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            while (!condition())
            {
                timeout.Token.ThrowIfCancellationRequested();
                await Task.Delay(20, timeout.Token);
            }
        }

        private sealed class NoopConnectionInitializer : IMongoConnectionInitializer
        {
            public ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult(new ConnectionInfo(new BsonDocument(), new BsonDocument()));
            }
        }

        private sealed class BlockingInitializer : IMongoConnectionInitializer
        {
            private readonly TaskCompletionSource _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public bool CancellationObserved { get; private set; }

            public async ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
            {
                _entered.TrySetResult();

                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    throw new InvalidOperationException("Initializer unexpectedly completed.");
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    CancellationObserved = true;
                    throw;
                }
            }

            public Task WaitUntilEnteredAsync() => _entered.Task;
        }

        private sealed class RecordingMongoConnectionFactory : IMongoConnectionFactory
        {
            private int _connectionId;

            public int CreateCalls { get; private set; }

            public List<MongoConnection> CreatedConnections { get; } = new();

            public List<MongoScheduler> RequestSchedulers { get; } = new();

            public ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                CreateCalls++;
                RequestSchedulers.Add(requestScheduler);

                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                CreatedConnections.Add(connection);
                return ValueTask.FromResult(connection);
            }
        }

        private sealed class BlockingMongoConnectionFactory : IMongoConnectionFactory
        {
            private readonly int _blockOnCallNumber;
            private readonly TaskCompletionSource _blocked = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _connectionId;

            public BlockingMongoConnectionFactory(int blockOnCallNumber)
            {
                _blockOnCallNumber = blockOnCallNumber;
            }

            public int CreateCalls { get; private set; }

            public List<MongoConnection> CreatedConnections { get; } = new();

            public bool CreateCanceled { get; private set; }

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                var callNumber = ++CreateCalls;
                if (callNumber == _blockOnCallNumber)
                {
                    _blocked.TrySetResult();
                    try
                    {
                        await _release.Task.WaitAsync(token);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        CreateCanceled = true;
                        throw;
                    }
                }

                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                CreatedConnections.Add(connection);
                return connection;
            }

            public Task WaitUntilBlockedAsync() => _blocked.Task;

            public void ReleaseBlockedCall() => _release.TrySetResult();
        }

        private sealed class FailOnReconnectMongoConnectionFactory : IMongoConnectionFactory
        {
            private int _connectionId;

            public int CreateCalls { get; private set; }

            public List<MongoConnection> CreatedConnections { get; } = new();

            public ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                CreateCalls++;
                if (CreateCalls > 1)
                {
                    throw new InvalidOperationException("reconnect failed");
                }

                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                CreatedConnections.Add(connection);
                return ValueTask.FromResult(connection);
            }
        }

        private sealed class MixedReconnectOutcomeMongoConnectionFactory : IMongoConnectionFactory
        {
            private readonly TaskCompletionSource _recoveringReconnectBlocked = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _recoveringReconnectRelease = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _connectionId;

            public int CreateCalls { get; private set; }

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                var callNumber = ++CreateCalls;
                if (callNumber == 3)
                {
                    await _recoveringReconnectBlocked.Task.WaitAsync(token);
                    throw new InvalidOperationException("reconnect failed");
                }

                if (callNumber == 4)
                {
                    _recoveringReconnectBlocked.TrySetResult();
                    await _recoveringReconnectRelease.Task.WaitAsync(token);
                }

                var transport = new TestTransport();
                var protocolReader = new ProtocolReader(transport.Input.Reader);
                var protocolWriter = new ProtocolWriter(transport.Output.Writer);
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, transport, token);
                return connection;
            }

            public Task WaitUntilRecoveringReconnectIsBlockedAsync() => _recoveringReconnectBlocked.Task;

            public void ReleaseRecoveringReconnect() => _recoveringReconnectRelease.TrySetResult();
        }

        private sealed class FaultedReplacementMongoConnectionFactory : IMongoConnectionFactory
        {
            private int _connectionId;

            public int CreateCalls { get; private set; }

            public List<MongoConnection> CreatedConnections { get; } = new();

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                CreateCalls++;

                var transport = new TestTransport();
                var protocolReader = new ProtocolReader(transport.Input.Reader);
                var protocolWriter = new ProtocolWriter(transport.Output.Writer);
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, transport, token);

                if (CreateCalls > 1)
                {
                    await transport.DisconnectAsync();
                    await WaitUntilAsync(() => IsFaultedOrDisposed(connection));
                }

                CreatedConnections.Add(connection);
                return connection;
            }
        }

        private sealed class DisposeDuringWarmupPublishMongoConnectionFactory : IMongoConnectionFactory
        {
            private readonly TaskCompletionSource _disposeStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _connectionId;
            private MongoScheduler? _scheduler;

            public Task? DisposeTask { get; private set; }

            public void AttachScheduler(MongoScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public async Task WaitUntilDisposeStartedAsync()
            {
                await _disposeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                if (_connectionId == settings.ConnectionPoolMaxSize)
                {
                    var scheduler = Assert.IsType<MongoScheduler>(_scheduler);
                    DisposeTask = Task.Run(async () =>
                    {
                        _disposeStarted.TrySetResult();
                        await scheduler.DisposeAsync().ConfigureAwait(false);
                    });

                    await WaitUntilLifecycleStateAsync(scheduler, SchedulerLifecycleState.Disposing, token);
                }

                return connection;
            }

            private static async Task WaitUntilLifecycleStateAsync(MongoScheduler scheduler, SchedulerLifecycleState expectedState, CancellationToken token)
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                while (GetLifecycleState(scheduler) != expectedState)
                {
                    await Task.Delay(20, timeoutCts.Token);
                }
            }
        }

        private enum SchedulerLifecycleState
        {
            Created = 0,
            Starting = 1,
            Started = 2,
            Disposing = 3,
            Disposed = 4,
            Failed = 5
        }

        private sealed class WarmupDisconnectMongoConnectionFactory : IMongoConnectionFactory
        {
            private readonly int _blockOnCallNumber;
            private readonly TaskCompletionSource _blocked = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private int _connectionId;

            public WarmupDisconnectMongoConnectionFactory(int blockOnCallNumber)
            {
                _blockOnCallNumber = blockOnCallNumber;
            }

            public int CreateCalls { get; private set; }

            public List<MongoConnection> CreatedConnections { get; } = new();

            public List<TestTransport> Transports { get; } = new();

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                var callNumber = ++CreateCalls;
                if (callNumber == _blockOnCallNumber)
                {
                    _blocked.TrySetResult();
                    await _release.Task.WaitAsync(token);
                }

                var transport = new TestTransport();
                var protocolReader = new ProtocolReader(transport.Input.Reader);
                var protocolWriter = new ProtocolWriter(transport.Output.Writer);
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, transport, token);

                Transports.Add(transport);
                CreatedConnections.Add(connection);
                return connection;
            }

            public Task WaitUntilBlockedAsync() => _blocked.Task;

            public void ReleaseBlockedCall() => _release.TrySetResult();
        }

        private sealed class InitializerAwareMongoConnectionFactory : IMongoConnectionFactory
        {
            private int _connectionId;

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                var transport = new TestTransport();
                var protocolReader = new ProtocolReader(transport.Input.Reader);
                var protocolWriter = new ProtocolWriter(transport.Output.Writer);
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, transport, token);
                return connection;
            }
        }

        private sealed class DisposeTrackingMongoConnectionFactory : IMongoConnectionFactory
        {
            private readonly int _throwOnDisposeConnectionId;
            private int _connectionId;

            public DisposeTrackingMongoConnectionFactory(int throwOnDisposeConnectionId)
            {
                _throwOnDisposeConnectionId = throwOnDisposeConnectionId;
            }

            public List<TrackingAsyncDisposable> Owners { get; } = new();

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                var transport = new TestTransport();
                var protocolReader = new ProtocolReader(transport.Input.Reader);
                var protocolWriter = new ProtocolWriter(transport.Output.Writer);
                var connectionId = Interlocked.Increment(ref _connectionId);
                var connection = new MongoConnection(
                    connectionId,
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                var owner = new TrackingAsyncDisposable(throwOnDispose: connectionId == _throwOnDisposeConnectionId);
                Owners.Add(owner);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, owner, token);
                return connection;
            }
        }

        private sealed class WriteFaultMongoConnectionFactory : IMongoConnectionFactory
        {
            private int _connectionId;

            public int CreateCalls { get; private set; }

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                CreateCalls++;

                var protocolReader = new ProtocolReader(new Pipe().Reader);
                var protocolWriter = CreateCalls == 1
                    ? new ProtocolWriter(new ThrowingPipeWriter())
                    : new ProtocolWriter(new Pipe().Writer);
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, token);
                return connection;
            }
        }

        private sealed class AlwaysDisconnectingWarmupConnectionFactory : IMongoConnectionFactory
        {
            private int _connectionId;

            public int CreateCalls { get; private set; }

            public async ValueTask<MongoConnection> CreateAsync(
                MongoClientSettings settings,
                IMongoConnectionInitializer initializer,
                ChannelReader<MongoRequest> reader,
                MongoScheduler requestScheduler,
                CancellationToken token)
            {
                CreateCalls++;

                var transport = new TestTransport();
                var protocolReader = new ProtocolReader(transport.Input.Reader);
                var protocolWriter = new ProtocolWriter(transport.Output.Writer);
                var connection = new MongoConnection(
                    Interlocked.Increment(ref _connectionId),
                    settings,
                    NullLogger.Instance,
                    reader,
                    requestScheduler);

                await connection.StartAsync(initializer, protocolReader, protocolWriter, transport, token);
                await transport.DisconnectAsync();
                await WaitUntilAsync(() => IsFaultedOrDisposed(connection));
                return connection;
            }
        }

        private sealed class TestTransport : IAsyncDisposable
        {
            private readonly Pipe _input = new();
            private readonly Pipe _output = new();
            private int _disposeState;

            public Pipe Input => _input;

            public Pipe Output => _output;

            public Task DisconnectAsync() => CompleteQuietlyAsync(_input.Writer);

            public async ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposeState, 1) != 0)
                {
                    return;
                }

                await CompleteQuietlyAsync(_input.Writer).ConfigureAwait(false);
                await CompleteQuietlyAsync(_input.Reader).ConfigureAwait(false);
                await CompleteQuietlyAsync(_output.Writer).ConfigureAwait(false);
                await CompleteQuietlyAsync(_output.Reader).ConfigureAwait(false);
            }

            private static async Task CompleteQuietlyAsync(PipeWriter writer)
            {
                try
                {
                    await writer.CompleteAsync().ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                }
            }

            private static async Task CompleteQuietlyAsync(PipeReader reader)
            {
                try
                {
                    await reader.CompleteAsync().ConfigureAwait(false);
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        private sealed class TrackingAsyncDisposable : IAsyncDisposable
        {
            private readonly bool _throwOnDispose;

            public TrackingAsyncDisposable(bool throwOnDispose = false)
            {
                _throwOnDispose = throwOnDispose;
            }

            public int DisposeCount { get; private set; }

            public ValueTask DisposeAsync()
            {
                DisposeCount++;
                return _throwOnDispose
                    ? ValueTask.FromException(new InvalidOperationException("cleanup failed"))
                    : ValueTask.CompletedTask;
            }
        }

        private sealed class ThrowingPipeWriter : PipeWriter
        {
            public override void Advance(int bytes)
            {
            }

            public override Memory<byte> GetMemory(int sizeHint = 0) => new byte[Math.Max(sizeHint, 1)];

            public override Span<byte> GetSpan(int sizeHint = 0) => new byte[Math.Max(sizeHint, 1)];

            public override void CancelPendingFlush()
            {
            }

            public override void Complete(Exception? exception = null)
            {
            }

            public override ValueTask CompleteAsync(Exception? exception = null) => ValueTask.CompletedTask;

            public override ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
            {
                return ValueTask.FromException<FlushResult>(new InvalidOperationException("write failed"));
            }
        }

        private sealed class NoopMessageWriter : IMessageWriter<object>
        {
            public static NoopMessageWriter Instance { get; } = new();

            public void WriteMessage(object message, IBufferWriter<byte> output)
            {
                var buffer = output.GetSpan(1);
                buffer[0] = 0x01;
                output.Advance(1);
            }
        }
    }
}
