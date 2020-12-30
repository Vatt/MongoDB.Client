using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client
{
    internal class TaskCompletionSourceWithCancellation<T> : TaskCompletionSource<T>
    {
        private CancellationToken _cancellationToken;

        public TaskCompletionSourceWithCancellation() : base(TaskCreationOptions.RunContinuationsAsynchronously)
        {
        }

        public TaskCompletionSourceWithCancellation(TaskCreationOptions options) : base(options)
        {
        }

        private void OnCancellation()
        {
            TrySetCanceled(_cancellationToken);
        }

        public async ValueTask<T> WaitWithCancellationAsync(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            using (cancellationToken.UnsafeRegister(s => ((TaskCompletionSourceWithCancellation<T>)s).OnCancellation(), this))
            {
                return await Task.ConfigureAwait(false);
            }
        }
    }
}
