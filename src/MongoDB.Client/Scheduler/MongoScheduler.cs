using Microsoft.Extensions.Logging;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Connection;
using MongoDB.Client.Exceptions;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Messages;
using MongoDB.Client.Scheduler.Holders;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Scheduler
{

    internal partial class MongoScheduler : IAsyncDisposable
    {
        private readonly IMongoConnectionFactory _connectionFactory;
        private readonly IMongoConnectionInitializer _connectionInitializer;
        private readonly ILogger<StandaloneScheduler> _logger;

        private readonly List<MongoConnection> _connections;
        private readonly Channel<MongoRequest> _channel;
        private readonly ChannelWriter<MongoRequest> _channelWriter;
        private readonly MongoClientSettings _settings;
        private readonly int _maxConnections;
        private static int _counter;
        private readonly SemaphoreSlim _lifecycleLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private readonly CancellationToken _shutdownToken;
        private int _lifecycleState = (int)SchedulerLifecycleState.Created;
        private int _inFlightReconnects;
        private Exception? _terminalRequestException;

        public MongoClusterTime? ClusterTime { get; protected set; }

        public MongoScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory, MongoClusterTime? clusterTime, IMongoConnectionInitializer connectionInitializer)
        {
            _connectionFactory = connectionFactory;
            _logger = loggerFactory.CreateLogger<StandaloneScheduler>();
            var options = new BoundedChannelOptions(10);
            _channel = Channel.CreateBounded<MongoRequest>(options);
            _channelWriter = _channel.Writer;
            _connections = new List<MongoConnection>();
            _settings = settings;
            _counter = 0;
            _maxConnections = settings.ConnectionPoolMaxSize;
            ClusterTime = clusterTime;
            _connectionInitializer = connectionInitializer;
            _shutdownToken = _shutdownCts.Token;
        }

        public MongoScheduler(MongoClientSettings settings, IMongoConnectionFactory connectionFactory, ILoggerFactory loggerFactory, IMongoConnectionInitializer connectionInitializer)
            : this(settings, connectionFactory, loggerFactory, null, connectionInitializer)
        {
        }


        public int GetNextRequestNumber()
        {
            return Interlocked.Increment(ref _counter);
        }


        public async ValueTask StartAsync(CancellationToken token)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _shutdownToken);

            try
            {
                await _lifecycleLock.WaitAsync(linkedCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
            {
                throw new ObjectDisposedException(nameof(MongoScheduler));
            }

            try
            {
                var state = (SchedulerLifecycleState)Volatile.Read(ref _lifecycleState);
                if (state == SchedulerLifecycleState.Started)
                {
                    return;
                }

                if (state == SchedulerLifecycleState.Failed)
                {
                    throw _terminalRequestException ?? new MongoException("Scheduler is in a failed state.");
                }

                if (state == SchedulerLifecycleState.Disposing || state == SchedulerLifecycleState.Disposed)
                {
                    throw new ObjectDisposedException(nameof(MongoScheduler));
                }

                Volatile.Write(ref _lifecycleState, (int)SchedulerLifecycleState.Starting);

                var createdConnections = new List<MongoConnection>(_maxConnections);
                var maxWarmupAttempts = Math.Max(_maxConnections * 2, 1);
                var warmupAttempts = 0;

                try
                {
                    while ((SchedulerLifecycleState)Volatile.Read(ref _lifecycleState) == SchedulerLifecycleState.Starting)
                    {
                        while ((SchedulerLifecycleState)Volatile.Read(ref _lifecycleState) == SchedulerLifecycleState.Starting &&
                            createdConnections.Count < _maxConnections &&
                            warmupAttempts < maxWarmupAttempts)
                        {
                            await RemoveUnusableConnectionsAsync(createdConnections).ConfigureAwait(false);

                            if (createdConnections.Count == _maxConnections)
                            {
                                break;
                            }

                            var connection = await CreateNewConnection(linkedCts.Token).ConfigureAwait(false);
                            createdConnections.Add(connection);
                            warmupAttempts++;
                        }

                        await RemoveUnusableConnectionsAsync(createdConnections).ConfigureAwait(false);

                        if (createdConnections.Count == _maxConnections)
                        {
                            if (!TryBeginWarmupPublish(createdConnections))
                            {
                                await RemoveUnusableConnectionsAsync(createdConnections).ConfigureAwait(false);
                                continue;
                            }

                            if (TryTransitionToStartedAndPublish(createdConnections))
                            {
                                return;
                            }

                            RollbackWarmupPublish(createdConnections);
                            await RemoveUnusableConnectionsAsync(createdConnections).ConfigureAwait(false);
                            continue;
                        }

                        if ((SchedulerLifecycleState)Volatile.Read(ref _lifecycleState) == SchedulerLifecycleState.Starting &&
                            warmupAttempts >= maxWarmupAttempts)
                        {
                            throw new MongoException($"Warmup failed after {warmupAttempts} connection attempts.");
                        }

                        if (createdConnections.Count < _maxConnections)
                        {
                            continue;
                        }
                    }
                }
                catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
                {
                    var capturedException = ExceptionDispatchInfo.Capture(new ObjectDisposedException(nameof(MongoScheduler)));
                    capturedException = await DisposeConnectionsBestEffortAsync(
                        createdConnections,
                        capturedException,
                        "Error on disposing warmup connection during shutdown").ConfigureAwait(false);
                    Volatile.Write(ref _lifecycleState, (int)SchedulerLifecycleState.Disposed);
                    capturedException.Throw();
                    throw new InvalidOperationException("Unreachable scheduler shutdown path.");
                }

                catch (Exception e)
                {
                    var capturedException = ExceptionDispatchInfo.Capture(e);
                    var restoredCreated = Interlocked.CompareExchange(
                        ref _lifecycleState,
                        (int)SchedulerLifecycleState.Created,
                        (int)SchedulerLifecycleState.Starting) == (int)SchedulerLifecycleState.Starting;

                    RollbackWarmupPublish(createdConnections);
                    capturedException = await DisposeConnectionsBestEffortAsync(
                        createdConnections,
                        capturedException,
                        "Error on disposing warmup connection after startup failure").ConfigureAwait(false);

                    if (!restoredCreated)
                    {
                        Volatile.Write(ref _lifecycleState, (int)SchedulerLifecycleState.Disposed);
                    }

                    capturedException.Throw();
                    throw new InvalidOperationException("Unreachable scheduler startup failure path.");
                }

                RollbackWarmupPublish(createdConnections);
                var disposedException = ExceptionDispatchInfo.Capture(new ObjectDisposedException(nameof(MongoScheduler)));
                disposedException = await DisposeConnectionsBestEffortAsync(
                    createdConnections,
                    disposedException,
                    "Error on disposing warmup connection during shutdown").ConfigureAwait(false);
                Volatile.Write(ref _lifecycleState, (int)SchedulerLifecycleState.Disposed);
                disposedException.Throw();
                throw new InvalidOperationException("Unreachable scheduler disposed path.");
            }
            finally
            {
                _lifecycleLock.Release();
            }
        }


        private ValueTask<MongoConnection> CreateNewConnection(CancellationToken token)
        {
            return _connectionFactory.CreateAsync(_settings, _connectionInitializer, _channel.Reader, this, token);
        }


        public async ValueTask<CursorResult<T>> GetCursorAsync<T>(FindMessage message, CancellationToken token)
            where T : IBsonSerializer<T>
        {
            var request = MongoRequestPool.Get();
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = CursorCallbackHolder<T>.CursorParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.FindMessageWriter, message, token);
            };
            request.RequestNumber = message.Header.RequestNumber;
            try
            {
                await EnqueueRequestAsync(request, token).ConfigureAwait(false);
                var cursor = (CursorResult<T>)await request.GetValueTask().ConfigureAwait(false);
                return cursor;
            }
            finally
            {
                MongoRequestPool.Return(request);
            }
        }


        public async ValueTask InsertAsync<T>(InsertMessage<T> message, CancellationToken token)
            where T : IBsonSerializer<T>
        {
            var request = MongoRequestPool.Get();
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = InsertCallbackHolder<T>.InsertParseAsync; //TODO: Try FIXIT
            request.WriteAsync = (protocol, token) =>
            {
                return InsertCallbackHolder<T>.WriteAsync(message, protocol, token);
            };
            InsertResult result;
            try
            {
                await EnqueueRequestAsync(request, token).ConfigureAwait(false);
                result = (InsertResult)await request.GetValueTask().ConfigureAwait(false);
            }
            finally
            {
                MongoRequestPool.Return(request);
            }

            if (result.WriteErrors is null || result.WriteErrors.Count == 0)
            {
                return;
            }

            ThrowHelper.InsertException(result.WriteErrors);
        }


        public async ValueTask<DeleteResult> DeleteAsync(DeleteMessage message, CancellationToken token)
        {
            var request = MongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = DeleteCallbackHolder.DeleteParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.DeleteMessageWriter, message, token);
            };
            try
            {
                await EnqueueRequestAsync(request, token).ConfigureAwait(false);
                var deleteResult = (DeleteResult)await request.GetValueTask().ConfigureAwait(false);
                return deleteResult!;
            }
            finally
            {
                MongoRequestPool.Return(request);
            }
        }
        public async ValueTask<UpdateResult> UpdateAsync(UpdateMessage message, CancellationToken token)
        {
            var request = MongoRequestPool.Get();//new DeleteMongoRequest(message, taskSource);
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = UpdateCallbackHolder.UpdateParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.UpdateMessageWriter, message, token);
            };
            UpdateResult updateResult;
            try
            {
                await EnqueueRequestAsync(request, token).ConfigureAwait(false);
                updateResult = (UpdateResult)await request.GetValueTask().ConfigureAwait(false);
            }
            finally
            {
                MongoRequestPool.Return(request);
            }

            return updateResult.ErrorMessage is null ? updateResult : ThrowHelper.UpdateException<UpdateResult>(updateResult.ErrorMessage);
        }
        public async ValueTask TransactionAsync(TransactionMessage message, CancellationToken token)
        {
            var request = MongoRequestPool.Get();
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = TransactionCallbackHolder.TransactionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.TransactionMessageWriter, message, token);
            };
            TransactionResult transactionResult;
            try
            {
                await EnqueueRequestAsync(request, token).ConfigureAwait(false);
                transactionResult = (TransactionResult)await request.GetValueTask().ConfigureAwait(false);
            }
            finally
            {
                MongoRequestPool.Return(request);
            }
            if (transactionResult!.Ok != 1)
            {
                ThrowHelper.TransactionException(transactionResult!.ErrorMessage!, transactionResult!.Code!.Value, transactionResult!.CodeName!);
            }
        }

        public async ValueTask DropCollectionAsync(DropCollectionMessage message, CancellationToken token)
        {
            var taskSource = new ManualResetValueTaskSource<IParserResult>();
            var request = new MongoRequest(taskSource);
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = DropCollectionCallbackHolder.DropCollectionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.DropCollectionMessageWriter, message, token);
            };
            await EnqueueRequestAsync(request, token).ConfigureAwait(false);
            var result = (DropCollectionResult)await request.GetValueTask().ConfigureAwait(false);

            if (result.Ok != 1)
            {
                ThrowHelper.DropCollectionException(result.ErrorMessage!, result.Code, result.CodeName);
            }
        }


        public async ValueTask CreateCollectionAsync(CreateCollectionMessage message, CancellationToken token)
        {
            var taskSource = new ManualResetValueTaskSource<IParserResult>();
            var request = new MongoRequest(taskSource);
            request.BeginOperation();
            request.RequestNumber = message.Header.RequestNumber;
            request.ParseAsync = CreateCollectionCallbackHolder.CreateCollectionParseAsync;
            request.WriteAsync = (protocol, token) =>
            {
                return protocol.WriteAsync(ProtocolWriters.CreateCollectionMessageWriter, message, token);
            };
            await EnqueueRequestAsync(request, token).ConfigureAwait(false);
            var result = (CreateCollectionResult)await request.GetValueTask().ConfigureAwait(false);

            if (result.Ok != 1)
            {
                ThrowHelper.CreateCollectionException(result.ErrorMessage!, result.Code, result.CodeName!);
            }
        }

        public async Task ConnectionLost(MongoConnection connection)
        {
            var state = (SchedulerLifecycleState)Volatile.Read(ref _lifecycleState);
            if (state == SchedulerLifecycleState.Starting)
            {
                await TryDisposeConnectionAsync(connection, "Error on disposing warmup connection").ConfigureAwait(false);
                return;
            }

            if (state != SchedulerLifecycleState.Started)
            {
                return;
            }

            MongoConnection? replacementConnection = null;
            var removed = false;
            var reconnectRegistered = false;
            Exception? reconnectFailure = null;

            try
            {
                await _lifecycleLock.WaitAsync(_shutdownToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
            {
                await TryDisposeConnectionAsync(connection, "Error on disposing connection during shutdown").ConfigureAwait(false);
                return;
            }
            catch (ObjectDisposedException)
            {
                await TryDisposeConnectionAsync(connection, "Error on disposing connection after teardown").ConfigureAwait(false);
                return;
            }

            try
            {
                if ((SchedulerLifecycleState)Volatile.Read(ref _lifecycleState) != SchedulerLifecycleState.Started)
                {
                    return;
                }

                await TryDisposeConnectionAsync(connection, "Error on disposing connection").ConfigureAwait(false);
                removed = _connections.Remove(connection);
                if (removed)
                {
                    Interlocked.Increment(ref _inFlightReconnects);
                    reconnectRegistered = true;
                }
            }
            finally
            {
                _lifecycleLock.Release();
            }

            if (!removed)
            {
                return;
            }

            try
            {
                replacementConnection = await CreateNewConnection(_shutdownToken).ConfigureAwait(false);
                if (!replacementConnection.TryBeginWarmupPublish())
                {
                    reconnectFailure = new MongoException("Replacement connection became unavailable before publish.");
                }
            }
            catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on creating connection");
                reconnectFailure = new MongoException("Scheduler lost its last connection and failed to reconnect.", e);
                return;
            }
            finally
            {
                try
                {
                    if (reconnectFailure is null && replacementConnection is not null)
                    {
                        try
                        {
                            await _lifecycleLock.WaitAsync(_shutdownToken).ConfigureAwait(false);
                            try
                            {
                                if ((SchedulerLifecycleState)Volatile.Read(ref _lifecycleState) == SchedulerLifecycleState.Started &&
                                    replacementConnection.TryCommitWarmupPublish())
                                {
                                    _connections.Add(replacementConnection);
                                    replacementConnection = null;
                                }
                                else
                                {
                                    reconnectFailure ??= new MongoException("Replacement connection was not healthy at publish point.");
                                }
                            }
                            finally
                            {
                                _lifecycleLock.Release();
                            }
                        }
                        catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
                        {
                        }
                        catch (ObjectDisposedException)
                        {
                        }
                    }
                }
                finally
                {
                    if (reconnectRegistered)
                    {
                        Interlocked.Decrement(ref _inFlightReconnects);
                    }

                    try
                    {
                        if (replacementConnection is not null)
                        {
                            replacementConnection.RollbackWarmupPublish();
                            await TryDisposeConnectionAsync(replacementConnection, "Error on disposing replacement connection").ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error on finalizing replacement connection");
                    }

                    if (reconnectFailure is not null)
                    {
                        await TransitionToFailedStateAsync(reconnectFailure).ConfigureAwait(false);
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            var previousState = (SchedulerLifecycleState)Interlocked.Exchange(ref _lifecycleState, (int)SchedulerLifecycleState.Disposing);
            if (previousState == SchedulerLifecycleState.Disposed || previousState == SchedulerLifecycleState.Disposing)
            {
                return;
            }

            _shutdownCts.Cancel();
            _terminalRequestException ??= CreateSchedulerDisposedException();
            _channelWriter.TryComplete(_terminalRequestException);

            ExceptionDispatchInfo? capturedException = null;
            await _lifecycleLock.WaitAsync().ConfigureAwait(false);
            try
            {
                capturedException = await DisposeConnectionsBestEffortAsync(
                    _connections,
                    capturedException,
                    "Error on disposing scheduler connection").ConfigureAwait(false);
                _connections.Clear();
                FailBufferedRequests(_terminalRequestException);
            }
            finally
            {
                Volatile.Write(ref _lifecycleState, (int)SchedulerLifecycleState.Disposed);
                _lifecycleLock.Release();
            }

            _lifecycleLock.Dispose();
            _shutdownCts.Dispose();
            capturedException?.Throw();
        }

        private async ValueTask RemoveUnusableConnectionsAsync(List<MongoConnection> connections)
        {
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                var connection = connections[i];
                if (!connection.IsFaultedOrDisposed)
                {
                    continue;
                }

                connections.RemoveAt(i);
                await TryDisposeConnectionAsync(connection, "Error on disposing unusable warmup connection").ConfigureAwait(false);
            }
        }

        private async ValueTask TryDisposeConnectionAsync(MongoConnection connection, string message)
        {
            try
            {
                await connection.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, message);
            }
        }

        private async ValueTask<ExceptionDispatchInfo?> DisposeConnectionsBestEffortAsync(
            IEnumerable<MongoConnection> connections,
            ExceptionDispatchInfo? capturedException,
            string errorMessage)
        {
            foreach (var connection in connections)
            {
                try
                {
                    await connection.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, errorMessage);
                    capturedException ??= ExceptionDispatchInfo.Capture(e);
                }
            }

            return capturedException;
        }

        private async ValueTask EnqueueRequestAsync(MongoRequest request, CancellationToken token)
        {
            var rejectionException = GetRequestRejectionException();
            if (rejectionException is not null)
            {
                request.TrySetException(rejectionException);
                throw rejectionException;
            }

            try
            {
                if (!_channelWriter.TryWrite(request))
                {
                    await _channelWriter.WriteAsync(request, token).ConfigureAwait(false);
                }
            }
            catch (ChannelClosedException)
            {
                rejectionException = GetRequestRejectionException() ?? new MongoException("Scheduler request channel is closed.");
                request.TrySetException(rejectionException);
                throw rejectionException;
            }
        }

        private Exception? GetRequestRejectionException()
        {
            var state = (SchedulerLifecycleState)Volatile.Read(ref _lifecycleState);
            if (state == SchedulerLifecycleState.Started)
            {
                return null;
            }

            if (state == SchedulerLifecycleState.Disposing || state == SchedulerLifecycleState.Disposed)
            {
                return _terminalRequestException ?? CreateSchedulerDisposedException();
            }

            if (state == SchedulerLifecycleState.Failed)
            {
                return _terminalRequestException ?? new MongoException("Scheduler has no active connections.");
            }

            return null;
        }

        private async ValueTask TransitionToFailedStateAsync(Exception exception)
        {
            Exception? bufferedFailure = null;

            try
            {
                await _lifecycleLock.WaitAsync(_shutdownToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_shutdownCts.IsCancellationRequested)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            try
            {
                if ((SchedulerLifecycleState)Volatile.Read(ref _lifecycleState) != SchedulerLifecycleState.Started ||
                    _connections.Count != 0 ||
                    Volatile.Read(ref _inFlightReconnects) != 0)
                {
                    return;
                }

                _terminalRequestException ??= exception;
                Volatile.Write(ref _lifecycleState, (int)SchedulerLifecycleState.Failed);
                _channelWriter.TryComplete(exception);
                bufferedFailure = exception;
            }
            finally
            {
                _lifecycleLock.Release();
            }

            if (bufferedFailure is not null)
            {
                FailBufferedRequests(bufferedFailure);
            }
        }

        private void FailBufferedRequests(Exception exception)
        {
            while (_channel.Reader.TryRead(out var request))
            {
                request.TrySetException(exception);
            }
        }

        private int GetPublishedConnectionCount()
        {
            try
            {
                return _connections.Count;
            }
            catch (ObjectDisposedException)
            {
                return 0;
            }
        }

        private static ObjectDisposedException CreateSchedulerDisposedException()
        {
            return new ObjectDisposedException(nameof(MongoScheduler));
        }

        private bool TryBeginWarmupPublish(List<MongoConnection> createdConnections)
        {
            for (var i = 0; i < createdConnections.Count; i++)
            {
                if (createdConnections[i].TryBeginWarmupPublish())
                {
                    continue;
                }

                for (var j = i - 1; j >= 0; j--)
                {
                    createdConnections[j].RollbackWarmupPublish();
                }

                return false;
            }

            return true;
        }

        private static void RollbackWarmupPublish(List<MongoConnection> createdConnections)
        {
            foreach (var connection in createdConnections)
            {
                connection.RollbackWarmupPublish();
            }
        }

        private bool TryTransitionToStartedAndPublish(List<MongoConnection> createdConnections)
        {
            if (Interlocked.CompareExchange(
                ref _lifecycleState,
                (int)SchedulerLifecycleState.Started,
                (int)SchedulerLifecycleState.Starting) != (int)SchedulerLifecycleState.Starting)
            {
                return false;
            }

            foreach (var connection in createdConnections)
            {
                if (!connection.TryCommitWarmupPublish())
                {
                    Interlocked.CompareExchange(
                        ref _lifecycleState,
                        (int)SchedulerLifecycleState.Starting,
                        (int)SchedulerLifecycleState.Started);
                    return false;
                }
            }

            _connections.AddRange(createdConnections);
            return true;
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
    }
}
