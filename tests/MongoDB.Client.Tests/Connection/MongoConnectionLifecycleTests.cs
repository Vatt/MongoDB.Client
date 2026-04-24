using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net;
using System.Reflection;
using System.Threading.Channels;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Client.Connection;
using MongoDB.Client.Messages;
using MongoDB.Client.Network.Transport.Abstractions;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;
using Xunit;

namespace MongoDB.Client.Tests.Connection
{
    public class MongoConnectionLifecycleTests
    {
        [Fact]
        public async Task StartAsync_WhenInitializerFails_DisposesProtocolResourcesAndStopsListener()
        {
            var scheduler = CreateScheduler();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(new Pipe().Reader);
            var writer = new ProtocolWriter(new Pipe().Writer);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await connection.StartAsync(new ThrowingInitializer(), reader, writer, CancellationToken.None));

            Assert.True(GetDisposedFlag(reader));
            Assert.True(GetDisposedFlag(writer));
            Assert.True(GetTask(connection, "_protocolListenerTask")!.IsCompleted);
            Assert.Null(GetTask(connection, "_channelListenerTask"));
        }

        [Fact]
        public async Task CreateAsync_WhenInitializerFails_DisposesConnectedContext()
        {
            var context = new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27017));
            var factory = new MongoConnectionFactory(
                context.RemoteEndPoint!,
                NullLoggerFactory.Instance,
                (endpoint, cancellationToken) => ValueTask.FromResult<Microsoft.AspNetCore.Connections.ConnectionContext>(context));

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await factory.CreateAsync(
                    new MongoClientSettings(),
                    new ThrowingInitializer(),
                    Channel.CreateUnbounded<MongoRequest>().Reader,
                    CreateScheduler(),
                    CancellationToken.None));

            Assert.Equal(1, context.DisposeCount);
        }

        [Fact]
        public async Task ExperimentalCreateAsync_WhenInitializerFails_DisposesConnectedContext()
        {
            var pipe = new TrackingDuplexPipe();
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27017);
            var factory = new Experimental.ExperimentalMongoConnectionFactory(
                endPoint,
                NullLoggerFactory.Instance,
                (endpoint, cancellationToken) => ValueTask.FromResult(System.Net.Connections.Connection.FromPipe(pipe, leaveOpen: false, remoteEndPoint: endpoint)));

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await factory.CreateAsync(
                    new MongoClientSettings(),
                    new ThrowingInitializer(),
                    Channel.CreateUnbounded<MongoRequest>().Reader,
                    CreateScheduler(),
                    CancellationToken.None));

            Assert.Equal(1, pipe.DisposeCount);
        }

        [Fact]
        public async Task CreateAsync_WhenDisposedAfterSuccessfulStart_DisposesConnectedContext()
        {
            var context = new TrackingConnectionContext(new IPEndPoint(IPAddress.Loopback, 27017));
            var factory = new MongoConnectionFactory(
                context.RemoteEndPoint!,
                NullLoggerFactory.Instance,
                (endpoint, cancellationToken) => ValueTask.FromResult<Microsoft.AspNetCore.Connections.ConnectionContext>(context));

            var connection = await factory.CreateAsync(
                new MongoClientSettings(),
                new NoopInitializer(),
                Channel.CreateUnbounded<MongoRequest>().Reader,
                CreateScheduler(),
                CancellationToken.None);

            await connection.DisposeAsync();

            Assert.Equal(1, context.DisposeCount);
        }

        [Fact]
        public async Task ExperimentalCreateAsync_WhenDisposedAfterSuccessfulStart_DisposesUnderlyingTransport()
        {
            var pipe = new TrackingDuplexPipe();
            var endPoint = new IPEndPoint(IPAddress.Loopback, 27017);
            var factory = new Experimental.ExperimentalMongoConnectionFactory(
                endPoint,
                NullLoggerFactory.Instance,
                (endpoint, cancellationToken) => ValueTask.FromResult(System.Net.Connections.Connection.FromPipe(pipe, leaveOpen: false, remoteEndPoint: endpoint)));

            var connection = await factory.CreateAsync(
                new MongoClientSettings(),
                new NoopInitializer(),
                Channel.CreateUnbounded<MongoRequest>().Reader,
                CreateScheduler(),
                CancellationToken.None);

            await connection.DisposeAsync();

            Assert.Equal(1, pipe.DisposeCount);
        }

        [Fact]
        public async Task DisposeAsync_WhenChannelListenerFaults_StillDisposesTransportOwner()
        {
            var scheduler = CreateScheduler();
            var channel = Channel.CreateUnbounded<MongoRequest>();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, channel.Reader, scheduler);
            var reader = new ProtocolReader(new Pipe().Reader);
            var writer = new ProtocolWriter(new ThrowingPipeWriter());
            var owner = new TrackingAsyncDisposable();

            connection.TakeTransportOwnership(owner);
            await connection.StartAsync(new NoopInitializer(), reader, writer, CancellationToken.None);

            var request = new MongoRequest(new ManualResetValueTaskSource<IParserResult>())
            {
                RequestNumber = 1,
                WriteAsync = (protocol, token) => protocol.WriteAsync(NoopMessageWriter.Instance, new object(), token),
                ParseAsync = static (_, _) => ValueTask.FromResult<IParserResult>(null!)
            };

            await channel.Writer.WriteAsync(request);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => request.CompletionSource.GetValueTask().AsTask());
            Assert.Equal("write failed", exception.Message);

            await connection.DisposeAsync();

            Assert.Equal(1, owner.DisposeCount);
        }

        [Fact]
        public async Task DisposeAsync_WhenCleanupFails_ThrowsCleanupException()
        {
            var scheduler = CreateScheduler();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(new Pipe().Reader);
            var writer = new ProtocolWriter(new Pipe().Writer);
            var owner = new ThrowingAsyncDisposable();

            await connection.StartAsync(new NoopInitializer(), reader, writer, owner, CancellationToken.None);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await connection.DisposeAsync());

            Assert.Equal("cleanup failed", exception.Message);
            Assert.Equal(1, owner.DisposeCount);
        }

        [Fact]
        public async Task StartAsync_WhenInitializerAndCleanupFail_ThrowsInitializerException()
        {
            var scheduler = CreateScheduler();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(new Pipe().Reader);
            var writer = new ProtocolWriter(new Pipe().Writer);
            var owner = new ThrowingAsyncDisposable();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await connection.StartAsync(new ThrowingInitializer(), reader, writer, owner, CancellationToken.None));

            Assert.Equal("initializer failed", exception.Message);
            Assert.Equal(1, owner.DisposeCount);
        }

        [Fact]
        public async Task DisposeAsync_FailsPendingQueryWithObjectDisposedException()
        {
            var scheduler = CreateScheduler();
            var transport = new TestProtocolTransport();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(transport.Input.Reader);
            var writer = new ProtocolWriter(transport.Output.Writer);

            await connection.StartAsync(new NoopInitializer(), reader, writer, transport, CancellationToken.None);

            var queryTask = connection.SendQueryAsync<MongoDB.Client.Bson.Document.BsonDocument>(
                "db",
                new MongoDB.Client.Bson.Document.BsonDocument(),
                CancellationToken.None).AsTask();

            await WaitUntilAsync(() => GetPendingCompletionCount(connection) == 1);

            await connection.DisposeAsync();

            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => queryTask);
            Assert.Equal(nameof(MongoConnection), exception.ObjectName);
            Assert.Equal(0, GetPendingCompletionCount(connection));
        }

        [Fact]
        public async Task DisposeAsync_WhenChannelListenerCancelsInFlightWrite_FailsRequestWithShutdown()
        {
            var scheduler = CreateScheduler();
            var channel = Channel.CreateUnbounded<MongoRequest>();
            var transport = new TestProtocolTransport();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, channel.Reader, scheduler);
            var reader = new ProtocolReader(transport.Input.Reader);
            var writer = new ProtocolWriter(transport.Output.Writer);
            var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var request = new MongoRequest(new ManualResetValueTaskSource<IParserResult>())
            {
                RequestNumber = 1,
                WriteAsync = async (_, token) =>
                {
                    writeStarted.TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                },
                ParseAsync = static (_, _) => ValueTask.FromResult<IParserResult>(null!)
            };

            request.BeginOperation();
            var completionTask = request.GetValueTask().AsTask();

            await connection.StartAsync(new NoopInitializer(), reader, writer, transport, CancellationToken.None);
            await channel.Writer.WriteAsync(request);
            await writeStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await WaitUntilAsync(() => GetPendingCompletionCount(connection) == 1);

            await connection.DisposeAsync();

            var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => completionTask.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(nameof(MongoConnection), exception.ObjectName);
            Assert.Equal(0, GetPendingCompletionCount(connection));
        }

        [Fact]
        public async Task ProtocolListener_WhenRemoteDisconnects_FailsPendingQuery()
        {
            var scheduler = CreateScheduler();
            var transport = new TestProtocolTransport();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(transport.Input.Reader);
            var writer = new ProtocolWriter(transport.Output.Writer);

            await connection.StartAsync(new NoopInitializer(), reader, writer, transport, CancellationToken.None);

            var queryTask = connection.SendQueryAsync<MongoDB.Client.Bson.Document.BsonDocument>(
                "db",
                new MongoDB.Client.Bson.Document.BsonDocument(),
                CancellationToken.None).AsTask();

            await WaitUntilAsync(() => GetPendingCompletionCount(connection) == 1);
            await transport.DisconnectAsync();

            await Assert.ThrowsAsync<MongoDB.Client.Exceptions.MongoException>(() => queryTask);
            Assert.Equal(0, GetPendingCompletionCount(connection));
        }

        [Fact]
        public async Task ProtocolListener_WhenProtocolFaultRacesWithShutdown_PendingQueryGetsProtocolFault()
        {
            var scheduler = CreateScheduler();
            var transport = new TestProtocolTransport();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(transport.Input.Reader);
            var writer = new ProtocolWriter(transport.Output.Writer);

            await connection.StartAsync(new NoopInitializer(), reader, writer, transport, CancellationToken.None);

            var queryTask = connection.SendQueryAsync<MongoDB.Client.Bson.Document.BsonDocument>(
                "db",
                new MongoDB.Client.Bson.Document.BsonDocument(),
                CancellationToken.None).AsTask();

            await WaitUntilAsync(() => GetPendingCompletionCount(connection) == 1);
            await WriteHeaderAsync(transport, messageLength: 16, requestId: 42, responseTo: 1, opcode: 9999);

            var exception = await Assert.ThrowsAsync<MongoDB.Client.Exceptions.MongoException>(() => queryTask.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal("Received broken data", exception.Message);
            Assert.IsNotType<ObjectDisposedException>(exception);
            Assert.Equal(0, GetPendingCompletionCount(connection));
        }

        [Fact]
        public async Task WarmupPublish_WhenConnectionFaultsBeforeCommit_DoesNotPromoteConnection()
        {
            var scheduler = CreateScheduler();
            var transport = new TestProtocolTransport();
            var connection = new MongoConnection(1, new MongoClientSettings(), NullLogger.Instance, Channel.CreateUnbounded<MongoRequest>().Reader, scheduler);
            var reader = new ProtocolReader(transport.Input.Reader);
            var writer = new ProtocolWriter(transport.Output.Writer);

            await connection.StartAsync(new NoopInitializer(), reader, writer, transport, CancellationToken.None);

            Assert.True(connection.TryBeginWarmupPublish());

            await transport.DisconnectAsync();
            await WaitUntilAsync(() => IsFaultedOrDisposed(connection));

            Assert.False(connection.TryCommitWarmupPublish());

            await connection.DisposeAsync();
        }

        [Fact]
        public async Task MongoRequest_StaleResultFromPreviousGeneration_DoesNotCompleteReusedSource()
        {
            var request = new MongoRequest(new ManualResetValueTaskSource<IParserResult>());

            var generation1 = request.BeginOperation();
            Assert.True(request.TrySetException(generation1, new InvalidOperationException("first generation failure")));
            await Assert.ThrowsAsync<InvalidOperationException>(() => request.GetValueTask().AsTask());

            var generation2 = request.BeginOperation();
            var secondGenerationTask = request.GetValueTask().AsTask();
            var expected = new TestParserResult();

            Assert.False(request.TrySetResult(generation1, new TestParserResult()));
            Assert.False(secondGenerationTask.IsCompleted);

            Assert.True(request.TrySetResult(generation2, expected));

            Assert.Same(expected, await secondGenerationTask);
        }

        [Fact]
        public async Task MongoRequest_StaleExceptionFromPreviousGeneration_DoesNotFailReusedSource()
        {
            var request = new MongoRequest(new ManualResetValueTaskSource<IParserResult>());

            var generation1 = request.BeginOperation();
            Assert.True(request.TrySetResult(generation1, new TestParserResult()));
            await request.GetValueTask().AsTask();

            var generation2 = request.BeginOperation();
            var secondGenerationTask = request.GetValueTask().AsTask();
            var expected = new TestParserResult();

            Assert.False(request.TrySetException(generation1, new ObjectDisposedException("stale")));
            Assert.False(secondGenerationTask.IsCompleted);

            Assert.True(request.TrySetResult(generation2, expected));

            Assert.Same(expected, await secondGenerationTask);
        }

        [Fact]
        public async Task ManualResetValueTaskSource_TrySetResult_DuplicateCompletion_DoesNotThrow()
        {
            var source = new ManualResetValueTaskSource<int>();
            var task = source.GetValueTask().AsTask();

            source.TrySetResult(42);
            var duplicate = Record.Exception(() => source.TrySetResult(43));

            Assert.Null(duplicate);
            Assert.Equal(42, await task);
        }

        [Fact]
        public async Task ManualResetValueTaskSource_TrySetException_DuplicateCompletion_DoesNotThrow()
        {
            var source = new ManualResetValueTaskSource<int>();
            var task = source.GetValueTask().AsTask();

            source.TrySetException(new InvalidOperationException("first"));
            var duplicate = Record.Exception(() => source.TrySetException(new InvalidOperationException("second")));

            Assert.Null(duplicate);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => task);
            Assert.Equal("first", exception.Message);
        }

        private static MongoScheduler CreateScheduler()
        {
            return new MongoScheduler(
                new MongoClientSettings(),
                new NoopMongoConnectionFactory(),
                NullLoggerFactory.Instance,
                new NoopInitializer());
        }

        private static bool GetDisposedFlag(object instance)
        {
            var field = instance.GetType().GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return Assert.IsType<bool>(field!.GetValue(instance));
        }

        private static Task? GetTask(MongoConnection connection, string fieldName)
        {
            var field = typeof(MongoConnection).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (Task?)field!.GetValue(connection);
        }

        private static int GetPendingCompletionCount(MongoConnection connection)
        {
            var field = typeof(MongoConnection).GetField("_completions", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return Assert.IsAssignableFrom<System.Collections.IDictionary>(field!.GetValue(connection)).Count;
        }

        private static bool IsFaultedOrDisposed(MongoConnection connection)
        {
            var property = typeof(MongoConnection).GetProperty("IsFaultedOrDisposed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(property);
            return Assert.IsType<bool>(property!.GetValue(connection));
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

        private static async Task WriteHeaderAsync(TestProtocolTransport transport, int messageLength, int requestId, int responseTo, int opcode)
        {
            var buffer = transport.Input.Writer.GetMemory(sizeof(int) * 4);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[0..4], messageLength);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[4..8], requestId);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[8..12], responseTo);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[12..16], opcode);
            transport.Input.Writer.Advance(sizeof(int) * 4);
            await transport.Input.Writer.FlushAsync();
        }

        private sealed class ThrowingInitializer : IMongoConnectionInitializer
        {
            public ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("initializer failed");
            }
        }

        private sealed class TestParserResult : IParserResult
        {
        }

        private sealed class NoopInitializer : IMongoConnectionInitializer
        {
            public ValueTask<ConnectionInfo> InitializeAsync(IMongoConnection connection, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult(new ConnectionInfo(new MongoDB.Client.Bson.Document.BsonDocument(), new MongoDB.Client.Bson.Document.BsonDocument()));
            }
        }

        private sealed class NoopMongoConnectionFactory : IMongoConnectionFactory
        {
            public ValueTask<MongoConnection> CreateAsync(MongoClientSettings settings, IMongoConnectionInitializer initializer, ChannelReader<MongoRequest> reader, MongoScheduler requestScheduler, CancellationToken token)
            {
                throw new NotSupportedException();
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

            public bool IsDisposed => DisposeCount > 0;

            public int DisposeCount { get; private set; }

            public override void Abort(Microsoft.AspNetCore.Connections.ConnectionAbortedException abortReason)
            {
                _connectionClosedSource.Cancel();
                base.Abort(abortReason);
            }

            public override ValueTask DisposeAsync()
            {
                DisposeCount++;
                _connectionClosedSource.Cancel();
                return ValueTask.CompletedTask;
            }
        }

        private sealed class TrackingDuplexPipe : IDuplexPipe, IAsyncDisposable
        {
            private readonly Pipe _input = new();
            private readonly Pipe _output = new();

            public PipeReader Input => _input.Reader;

            public PipeWriter Output => _output.Writer;

            public bool IsDisposed => DisposeCount > 0;

            public int DisposeCount { get; private set; }

            public async ValueTask DisposeAsync()
            {
                DisposeCount++;
                await _input.Reader.CompleteAsync().ConfigureAwait(false);
                await _output.Writer.CompleteAsync().ConfigureAwait(false);
            }
        }

        private sealed class TrackingAsyncDisposable : IAsyncDisposable
        {
            public int DisposeCount { get; private set; }

            public ValueTask DisposeAsync()
            {
                DisposeCount++;
                return ValueTask.CompletedTask;
            }
        }

        private sealed class ThrowingAsyncDisposable : IAsyncDisposable
        {
            public int DisposeCount { get; private set; }

            public ValueTask DisposeAsync()
            {
                DisposeCount++;
                throw new InvalidOperationException("cleanup failed");
            }
        }

        private sealed class TestProtocolTransport : IAsyncDisposable
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
