using Microsoft.Extensions.ObjectPool;
using MongoDB.Client.Messages;
using System;

namespace MongoDB.Client.Connection
{
    internal partial class RequestScheduler
    {
        private class FindRequestMessagePolicy : IPooledObjectPolicy<FindMongoRequest>
        {
            public FindMongoRequest Create()
            {
                return new FindMongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(FindMongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default;
                obj.Message = default;
                obj.ParseAsync = default;
                return true;
            }
        }
        private class InsertRequestMessagePolicy : IPooledObjectPolicy<InsertMongoRequest>
        {
            public InsertMongoRequest Create()
            {
                return new InsertMongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(InsertMongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default;
                obj.Message = default;
                obj.RequestNumber = default;
                obj.ParseAsync = default;
                obj.WriteAsync = default;
                return true;
            }
        }
        private class DeleteRequestMessagePolicy : IPooledObjectPolicy<DeleteMongoRequest>
        {
            public DeleteMongoRequest Create()
            {
                return new DeleteMongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(DeleteMongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default;
                obj.Message = default;
                obj.ParseAsync = default;
                return true;
            }
        }
        private static ObjectPool<FindMongoRequest> FindMongoRequestPool => _findMongoRequestPool ??= new DefaultObjectPool<FindMongoRequest>(new FindRequestMessagePolicy());
        [ThreadStatic]
        private static ObjectPool<FindMongoRequest>? _findMongoRequestPool;

        private static ObjectPool<InsertMongoRequest> InsertMongoRequestPool => _insertMongoRequestPool ??= new DefaultObjectPool<InsertMongoRequest>(new InsertRequestMessagePolicy());
        [ThreadStatic]
        private static ObjectPool<InsertMongoRequest>? _insertMongoRequestPool;

        private static ObjectPool<DeleteMongoRequest> DeleteMongoRequestPool => _deleteMongoRequestPool ??= new DefaultObjectPool<DeleteMongoRequest>(new DeleteRequestMessagePolicy());
        [ThreadStatic]
        private static ObjectPool<DeleteMongoRequest>? _deleteMongoRequestPool;
    }
}
