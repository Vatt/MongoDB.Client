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
        DeleteRequest,
        DropCollectionRequest
    }
    internal abstract class MongoReuqestBase
    {
        public RequestType Type { get; init; }
        internal int RequestNumber;
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

        public FindMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.FindRequest;
        }
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
        internal IMongoInsertMessage Message;
        public Func<IMongoInsertMessage, ProtocolWriter, CancellationToken, ValueTask> WriteAsync { get; set; }
        public InsertMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.InsertRequest;
        }
        public InsertMongoRequest(int requestNumber, IMongoInsertMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            RequestNumber = requestNumber;
            Message = message;
            Type = RequestType.InsertRequest;
        }
    }
    internal class DeleteMongoRequest : MongoReuqestBase
    {
        internal DeleteMessage Message;
        public DeleteMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.DeleteRequest;
        }
        public DeleteMongoRequest(DeleteMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.DeleteRequest;
            Message = message;
        }
    }
    
    internal class DropCollectionMongoRequest : MongoReuqestBase
    {
        internal DropCollectionMessage Message;
        public DropCollectionMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.DropCollectionRequest;
        }
        public DropCollectionMongoRequest(DropCollectionMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Type = RequestType.DropCollectionRequest;
            Message = message;
        }
    }
}
