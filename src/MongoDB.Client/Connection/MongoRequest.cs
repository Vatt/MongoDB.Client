using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;

namespace MongoDB.Client.Connection
{
    internal class MongoRequest
    {
        private long _completionState;

        internal int RequestNumber;
        public MongoRequest(ManualResetValueTaskSource<IParserResult> completionSource)
        {
            CompletionSource = completionSource;
        }

        public ManualResetValueTaskSource<IParserResult> CompletionSource { get; }
        public Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>>? ParseAsync { get; set; } //TODO: FIXIT
        public Func<ProtocolWriter, CancellationToken, ValueTask>? WriteAsync { get; set; }

        public long CurrentGeneration => Volatile.Read(ref _completionState) >> 1;

        public long BeginOperation()
        {
            CompletionSource.Reset();

            while (true)
            {
                var currentState = Volatile.Read(ref _completionState);
                var nextGeneration = (currentState >> 1) + 1;
                var nextState = nextGeneration << 1;
                if (Interlocked.CompareExchange(ref _completionState, nextState, currentState) == currentState)
                {
                    return nextGeneration;
                }
            }
        }

        public ValueTask<IParserResult> GetValueTask() => CompletionSource.GetValueTask();

        public bool TrySetResult(long generation, IParserResult result)
        {
            if (!TryBeginCompletion(generation))
            {
                return false;
            }

            CompletionSource.SetResult(result);
            return true;
        }

        public bool TrySetException(long generation, Exception error)
        {
            if (!TryBeginCompletion(generation))
            {
                return false;
            }

            CompletionSource.SetException(error);
            return true;
        }

        public bool TrySetException(Exception error) => TrySetException(CurrentGeneration, error);

        public void ResetForPool()
        {
            RequestNumber = default;
            ParseAsync = default;
            WriteAsync = default;
        }

        private bool TryBeginCompletion(long generation)
        {
            var expectedState = generation << 1;
            return Interlocked.CompareExchange(ref _completionState, expectedState | 1, expectedState) == expectedState;
        }
    }
}
