using MongoDB.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Protocol.Core
{
    public class ProtocolWriter : IAsyncDisposable
    {
        private readonly PipeWriter _writer;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed;


        public ProtocolWriter(Stream stream) :
            this(PipeWriter.Create(stream))
        {

        }


        public ProtocolWriter(PipeWriter writer)
            : this(writer, new SemaphoreSlim(1))
        {
        }


        public ProtocolWriter(PipeWriter writer, SemaphoreSlim semaphore)
        {
            _writer = writer;
            _semaphore = semaphore;
        }

        public async ValueTask WriteUnsafeAsync<TWriteMessage>(IMessageWriter<TWriteMessage> writer, TWriteMessage protocolMessage, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                return;
            }

            writer.WriteMessage(protocolMessage, _writer);

            var result = await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);

            if (result.IsCanceled)
            {
                ThrowHelper.CancelledException();
            }

            if (result.IsCompleted)
            {
                _disposed = true;
            }
        }
        public async ValueTask WriteAsync<TWriteMessage>(IMessageWriter<TWriteMessage> writer, TWriteMessage protocolMessage, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                writer.WriteMessage(protocolMessage, _writer);
           
                var result = await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    ThrowHelper.CancelledException();
                }

                if (result.IsCompleted)
                {
                    _disposed = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async ValueTask WriteAsync3<T0, T1, T2>(
            IMessageWriter<T0> writer0, T0 msg0,
            IMessageWriter<T1> writer1, T1 msg1,
            IMessageWriter<T2> writer2, T2 msg2,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                writer0.WriteMessage(msg0, _writer);
                writer1.WriteMessage(msg1, _writer);
                writer2.WriteMessage(msg2, _writer);

                var result = await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                if (result.IsCompleted)
                {
                    _disposed = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async ValueTask WriteAsync2<T0, T1>(
            IMessageWriter<T0> writer0, T0 msg0,
            IMessageWriter<T1> writer1, T1 msg1,
            CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                writer0.WriteMessage(msg0, _writer);
                writer1.WriteMessage(msg1, _writer);

                var result = await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                if (result.IsCompleted)
                {
                    _disposed = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async ValueTask WriteManyAsync<TWriteMessage>(IMessageWriter<TWriteMessage> writer, IEnumerable<TWriteMessage> protocolMessages, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                foreach (var protocolMessage in protocolMessages)
                {
                    writer.WriteMessage(protocolMessage, _writer);
                }

                var result = await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                if (result.IsCompleted)
                {
                    _disposed = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async ValueTask DisposeAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}