using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal abstract class MongoReuqestBase
    {
        public MongoReuqestBase(ManualResetValueTaskSource<IParserResult> completionSource)
        {
            CompletionSource = completionSource;
        }
        public ManualResetValueTaskSource<IParserResult> CompletionSource { get; }
        public Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> ParseAsync { get; set; } //TODO: FIXIT
    }
    internal class FindMongoRequest : MongoReuqestBase
    {
        internal FindMessage Message;
        public FindMongoRequest(FindMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Message = message;
        }
    }
    internal class QueryMongoRequest : MongoReuqestBase
    {
        internal QueryMessage Message;
        public QueryMongoRequest(QueryMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Message = message;
        }
    }
}
