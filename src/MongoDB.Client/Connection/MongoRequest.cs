using MongoDB.Client.Bson.Writer;
using MongoDB.Client.Messages;
using MongoDB.Client.Protocol;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Reader;

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
