using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Client.Messages;
using MongoDB.Client.Scheduler;

namespace MongoDB.Client
{
    public sealed class TransactionHandler : IAsyncDisposable
    {
        private static long _transactionCounter;
        private static readonly SessionId ImplicitSession = new SessionId();


        private readonly IMongoScheduler _scheduler;
        public long TxNumber { get; }
        public SessionId SessionId { get; }
        public TransactionState State { get; internal set; }

        internal TransactionHandler(long transactionNumber, TransactionState state, SessionId sessionId, IMongoScheduler scheduler)
        {
            State = state;
            TxNumber = transactionNumber;
            _scheduler = scheduler;
            SessionId = sessionId;
        }

        internal static TransactionHandler CreateImplicit(IMongoScheduler scheduler)
        {
            var number = Interlocked.Increment(ref _transactionCounter);
            return new TransactionHandler(number, TransactionState.Implicit, ImplicitSession, scheduler);
        }

        internal static TransactionHandler Create(IMongoScheduler scheduler)
        {
            var number = Interlocked.Increment(ref _transactionCounter);
            return new TransactionHandler(number, TransactionState.Starting, new SessionId(), scheduler);
        }


        public async ValueTask CommitAsync(CancellationToken cancellationToken = default)
        {
            await _scheduler.CommitTransactionAsync(this, cancellationToken).ConfigureAwait(false);
            State = TransactionState.Committed;
        }


        public async ValueTask AbortAsync(CancellationToken cancellationToken = default)
        {
            await _scheduler.AbortTransactionAsync(this, cancellationToken).ConfigureAwait(false);
            State = TransactionState.Aborted;
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
