using System.Diagnostics;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace MongoDB.Client.Network.Transport.Sockets.Internal
{
    internal sealed class SocketAwaitableEventArgs : SocketAsyncEventArgs, ICriticalNotifyCompletion
    {
        private static readonly Action _callbackCompleted = () => { };

        private readonly PipeScheduler _ioScheduler;

        private Action? _callback;

        public SocketAwaitableEventArgs(PipeScheduler ioScheduler)
            : base(unsafeSuppressExecutionContextFlow: true)
        {
            _ioScheduler = ioScheduler;
        }

        public SocketAwaitableEventArgs GetAwaiter() => this;
        public bool IsCompleted => ReferenceEquals(_callback, _callbackCompleted);

        public int GetResult()
        {
            Debug.Assert(ReferenceEquals(_callback, _callbackCompleted));

            _callback = null;

            if (SocketError != SocketError.Success)
            {
                ThrowSocketException(SocketError);
            }

            return BytesTransferred;

            static void ThrowSocketException(SocketError e)
            {
                throw new SocketException((int)e);
            }
        }

        public void OnCompleted(Action continuation)
        {
            if (ReferenceEquals(_callback, _callbackCompleted) ||
                ReferenceEquals(Interlocked.CompareExchange(ref _callback, continuation, null), _callbackCompleted))
            {
                Task.Run(continuation);
            }
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void Complete()
        {
            OnCompleted(this);
        }

        protected override void OnCompleted(SocketAsyncEventArgs _)
        {
            var continuation = Interlocked.Exchange(ref _callback, _callbackCompleted);

            if (continuation != null)
            {
                _ioScheduler.Schedule(state => ((Action)state!)(), continuation);
            }
        }
    }
}
