using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal enum RequestType : byte
    {
        FindRequest,
        QueryRequest,
        InsertRequest,
    }
    internal abstract class MongoReuqestBase
    {
        public RequestType Type { get; init; }
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
            Type = RequestType.FindRequest;
            Message = message;
        }
    }
    internal class QueryMongoRequest : MongoReuqestBase
    {
        internal QueryMessage Message;
        public QueryMongoRequest(QueryMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.QueryRequest;
            Message = message;
        }
    }
    internal class InsertMongoRequest : MongoReuqestBase
    {
        internal object Message;
        internal int RequestNumber;
        public Func<object, ProtocolWriter, CancellationToken, ValueTask> WriteAsync { get; set; }
        public InsertMongoRequest(int requestNumber, object message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            RequestNumber = requestNumber;
            Message = message;
            Type = RequestType.InsertRequest;
        }
    }
}
