using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal class MongoRequest
    {
        internal int RequestNumber;
        public MongoRequest(ManualResetValueTaskSource<IParserResult> completionSource)
        {
            CompletionSource = completionSource;
        }
        public ManualResetValueTaskSource<IParserResult> CompletionSource { get; }
        public Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> ParseAsync { get; set; } //TODO: FIXIT
        public Func<ProtocolWriter, CancellationToken, ValueTask> WriteAsync { get; set; }
    }
}
