using Microsoft.Extensions.ObjectPool;
using MongoDB.Client.Messages;
using System;

namespace MongoDB.Client.Connection
{
    internal partial class RequestScheduler
    {
        private class MongoRequestPolicy : IPooledObjectPolicy<MongoRequest>
        {
            public MongoRequest Create()
            {
                return new MongoRequest(new ManualResetValueTaskSource<IParserResult>());
            }

            public bool Return(MongoRequest obj)
            {
                obj.CompletionSource.Reset();
                obj.RequestNumber = default; ;
                obj.ParseAsync = default;
                obj.WriteAsync = default;
                return true;
            }
        }
        private static ObjectPool<MongoRequest> MongoRequestPool => _mongoRequestPool ??= new DefaultObjectPool<MongoRequest>(new MongoRequestPolicy());
        [ThreadStatic]
        private static ObjectPool<MongoRequest>? _mongoRequestPool;
    }
}
