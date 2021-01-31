using Microsoft.Extensions.ObjectPool;
using MongoDB.Client.Messages;
using System;

namespace MongoDB.Client.Connection
{
    internal partial class RequestScheduler
    {
        private class Policy : IPooledObjectPolicy<ManualResetValueTaskSource<IParserResult>>
        {
            public ManualResetValueTaskSource<IParserResult> Create()
            {
                return new ManualResetValueTaskSource<IParserResult>();
            }

            public bool Return(ManualResetValueTaskSource<IParserResult> obj)
            {
                obj.Reset();
                return true;
            }
        }
        private static ObjectPool<ManualResetValueTaskSource<IParserResult>> TaskSrcPool => _taskSrcPool ??= new DefaultObjectPool<ManualResetValueTaskSource<IParserResult>>(new Policy());
        [ThreadStatic]
        private static ObjectPool<ManualResetValueTaskSource<IParserResult>>? _taskSrcPool;
    }
}
