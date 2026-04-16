using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Scheduler;
using MongoDB.Client.Settings;

namespace MongoDB.Client.Connection
{
    public sealed partial class MongoConnection : IAsyncDisposable
    {
        public int ConnectionId { get; }
        private ILogger _logger;
        private ConcurrentDictionary<long, MongoRequest> _completions;
        private ProtocolReader? _protocolReader;
        private ProtocolWriter? _protocolWriter;
        private IAsyncDisposable? _transportOwner;
        private readonly ChannelReader<MongoRequest> _channelReader;
        //private readonly ChannelReader<MongoRequest> _findReader;
        private readonly MongoScheduler _requestScheduler;
        private CancellationTokenSource _shutdownCts = new CancellationTokenSource();
        private readonly CancellationToken _shutdownToken;
        private Task? _protocolListenerTask;
        private Task? _channelListenerTask;
        private int _disposeState;
        private int _faultedState;
        private int _poolState = (int)PoolState.Warmup;
        private Exception? _terminalException;
        private volatile bool _suppressConnectionLost;
        private readonly ConcurrentQueue<MongoRequest> _queue = new();
        private readonly MongoClientSettings _settings;

        internal MongoConnection(int connectionId, MongoClientSettings settings, ILogger logger, ChannelReader<MongoRequest> channelReader, MongoScheduler requestScheduler)
        {
            ConnectionId = connectionId;
            _completions = new ConcurrentDictionary<long, MongoRequest>();
            _logger = logger;
            _channelReader = channelReader;
            _requestScheduler = requestScheduler;
            _settings = settings;
            _shutdownToken = _shutdownCts.Token;
        }

        internal void TakeTransportOwnership(IAsyncDisposable owner)
        {
            ArgumentNullException.ThrowIfNull(owner);

            if (Interlocked.CompareExchange(ref _transportOwner, owner, null) is not null)
            {
                throw new InvalidOperationException("Transport owner is already assigned.");
            }
        }

        internal bool IsFaultedOrDisposed =>
            Volatile.Read(ref _faultedState) != 0 || Volatile.Read(ref _disposeState) != 0;

        internal bool TryBeginWarmupPublish()
        {
            return Interlocked.CompareExchange(
                ref _poolState,
                (int)PoolState.Publishing,
                (int)PoolState.Warmup) == (int)PoolState.Warmup;
        }

        internal bool TryCommitWarmupPublish()
        {
            return Interlocked.CompareExchange(
                ref _poolState,
                (int)PoolState.Published,
                (int)PoolState.Publishing) == (int)PoolState.Publishing;
        }

        internal void RollbackWarmupPublish()
        {
            while (true)
            {
                var state = (PoolState)Volatile.Read(ref _poolState);
                if (state is PoolState.Warmup or PoolState.Retired)
                {
                    return;
                }

                if (state is not PoolState.Publishing and not PoolState.Published)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _poolState, (int)PoolState.Warmup, (int)state) == (int)state)
                {
                    return;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposeState, 1) != 0)
            {
                return;
            }

            MarkRetiredBeforePublish();
            _suppressConnectionLost = true;
            TrySetTerminalException(CreateShutdownException());
            _shutdownCts.Cancel();
            FailPendingCompletions(GetTerminalException());
            ExceptionDispatchInfo? capturedException = null;

            capturedException = await TryAwaitAsync(_channelListenerTask, capturedException).ConfigureAwait(false);
            FailPendingCompletions(GetTerminalException());
            capturedException = await TryDisposeAsync(_protocolWriter, capturedException).ConfigureAwait(false);
            capturedException = await TryDisposeAsync(_protocolReader, capturedException).ConfigureAwait(false);
            capturedException = await TryDisposeAsync(Interlocked.Exchange(ref _transportOwner, null), capturedException).ConfigureAwait(false);
            capturedException = await TryAwaitAsync(_protocolListenerTask, capturedException).ConfigureAwait(false);
            FailPendingCompletions(GetTerminalException());
            capturedException?.Throw();
        }

        private async ValueTask<ExceptionDispatchInfo?> TryDisposeAsync(IAsyncDisposable? disposable, ExceptionDispatchInfo? capturedException)
        {
            if (disposable is null)
            {
                return capturedException;
            }

            try
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on disposing connection resource");
                capturedException ??= ExceptionDispatchInfo.Capture(e);
            }

            return capturedException;
        }

        private async ValueTask<ExceptionDispatchInfo?> TryAwaitAsync(Task? task, ExceptionDispatchInfo? capturedException)
        {
            if (task is null)
            {
                return capturedException;
            }

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error on awaiting connection listener");
                capturedException ??= ExceptionDispatchInfo.Capture(e);
            }

            return capturedException;
        }

        private void FailPendingCompletions(Exception exception)
        {
            MongoRequest? request;
            foreach (var key in _completions.Keys)
            {
                if (_completions.TryRemove(key, out request))
                {
                    request.TrySetException(exception);
                }
            }
        }

        private static Exception CreateShutdownException()
        {
            return new ObjectDisposedException(nameof(MongoConnection));
        }

        private void TrySetTerminalException(Exception exception)
        {
            Interlocked.CompareExchange(ref _terminalException, exception, null);
        }

        private Exception GetTerminalException()
        {
            return Volatile.Read(ref _terminalException) ?? CreateShutdownException();
        }

        private bool MarkFaulted(Exception exception)
        {
            MarkRetiredBeforePublish();

            if (Interlocked.Exchange(ref _faultedState, 1) != 0)
            {
                return false;
            }

            TrySetTerminalException(exception);
            FailPendingCompletions(GetTerminalException());
            _shutdownCts.Cancel();
            return true;
        }

        private void HandleListenerFault(Exception exception)
        {
            if (!MarkFaulted(exception))
            {
                return;
            }

            if (!_suppressConnectionLost)
            {
                _ = _requestScheduler.ConnectionLost(this);
            }
        }

        private void MarkRetiredBeforePublish()
        {
            while (true)
            {
                var state = (PoolState)Volatile.Read(ref _poolState);
                if (state is PoolState.Published or PoolState.Retired)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref _poolState, (int)PoolState.Retired, (int)state) == (int)state)
                {
                    return;
                }
            }
        }

        private enum PoolState
        {
            Warmup = 0,
            Publishing = 1,
            Published = 2,
            Retired = 3
        }
    }
}
