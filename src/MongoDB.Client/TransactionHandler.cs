using MongoDB.Client.Messages;
using MongoDB.Client.Scheduler;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public sealed class TransactionHandler : IAsyncDisposable
    {
        private static long _transactionCounter;

        internal TransactionHandler(long transactionNumber, TransactionState state, IMongoScheduler scheduler)
        {
            State = state;
            TxNumber = transactionNumber;
            _scheduler = scheduler;
        }

        internal static TransactionHandler CreateImplicit(IMongoScheduler scheduler)
        {
            var number = Interlocked.Increment(ref _transactionCounter);
            return new TransactionHandler(number, TransactionState.Implicit, scheduler);
        }

        internal static TransactionHandler Create(IMongoScheduler scheduler)
        {
            var number = Interlocked.Increment(ref _transactionCounter);
            return new TransactionHandler(number, TransactionState.Starting, scheduler);
        }

        public long TxNumber { get; }

        public TransactionState State { get; internal set; }

        private readonly IMongoScheduler _scheduler;

        public async ValueTask CommitAsync(CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var transactionRequest = CreateCommitRequest();
            var request = new TransactionMessage(requestNumber, transactionRequest);
            await _scheduler.TransactionAsync(request, cancellationToken).ConfigureAwait(false);
            State = TransactionState.Committed;
        }

        public async ValueTask AbortAsync(CancellationToken cancellationToken = default)
        {
            var requestNumber = _scheduler.GetNextRequestNumber();
            var transactionRequest = CreateAbortRequest();
            var request = new TransactionMessage(requestNumber, transactionRequest);
            await _scheduler.TransactionAsync(request, cancellationToken).ConfigureAwait(false);
            State = TransactionState.Aborted;
        }

        private TransactionRequest CreateCommitRequest()
        {
            return new TransactionRequest(1, null, "admin", _scheduler.SessionId, _scheduler.ClusterTime, TxNumber, false);
        }

        private TransactionRequest CreateAbortRequest()
        {
            return new TransactionRequest(null, 1, "admin", _scheduler.SessionId, _scheduler.ClusterTime, TxNumber, false);
        }

        public ValueTask DisposeAsync()
        {
            switch (State)
            {
                case TransactionState.InProgress:
                    return AbortAsync();
                case TransactionState.Committed:
                case TransactionState.Starting:
                case TransactionState.Aborted:
                case TransactionState.Implicit:
                default:
                    return default;
            }
        }
    }

    public enum TransactionState
    {
        Starting = 1,

        InProgress = 2,

        Committed = 3,

        Aborted = 4,

        Implicit = 5
    }
}
