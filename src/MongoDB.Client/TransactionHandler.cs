using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    public struct TransactionHandler
    {
        private static long _transactionCounter;

        internal TransactionHandler(long transactionNumber, TransactionState state)
        {
            State = state;
            TxNumber = transactionNumber;
        }

        public static TransactionHandler CreateImplicit()
        {
            var number = Interlocked.Increment(ref _transactionCounter);
            return new TransactionHandler(number, TransactionState.Implicit);
        }

        public static TransactionHandler Create()
        {
            var number = Interlocked.Increment(ref _transactionCounter);
            return new TransactionHandler(number, TransactionState.Starting);
        }

        public long TxNumber { get; }

        public TransactionState State { get; internal set; }

        public ValueTask CommitAsync()
        {
            return default;
        }

        public ValueTask AbortAsync()
        {
            return default;
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
