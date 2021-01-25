using MongoDB.Client.Messages;
using MongoDB.Client.Protocol.Core;
using MongoDB.Client.Protocol.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Client.Connection
{
    internal abstract class MongoRequestBase
    {
        internal int RequestNumber;
        public MongoRequestBase(ManualResetValueTaskSource<IParserResult> completionSource)
        {
            CompletionSource = completionSource;
        }
        public ManualResetValueTaskSource<IParserResult> CompletionSource { get; }
        public Func<ProtocolReader, MongoResponseMessage, ValueTask<IParserResult>> ParseAsync { get; set; } //TODO: FIXIT
        public Func<MongoRequestBase, ProtocolWriter, CancellationToken, ValueTask> WriteAsync { get; set; }
    }
    internal class FindMongoRequest : MongoRequestBase
    {
        internal FindMessage Message;

        public FindMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
        }
    }
    internal class QueryMongoRequest : MongoRequestBase
    {
        internal QueryMessage Message;
        public QueryMongoRequest(QueryMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Message = message;
        }
    }
    internal class InsertMongoRequest : MongoRequestBase
    {
        internal IMongoInsertMessage Message;
        public InsertMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
        }
        public InsertMongoRequest(int requestNumber, IMongoInsertMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            RequestNumber = requestNumber;
            Message = message;
        }
    }
    internal class DeleteMongoRequest : MongoRequestBase
    {
        internal DeleteMessage Message;
        public DeleteMongoRequest(ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            ParseAsync = DeleteCallbackHolder.DeleteParseAsync;
            WriteAsync = DeleteCallbackHolder.WriteAsync;
        }
    }
    
    internal class DropCollectionMongoRequest : MongoRequestBase
    {
        internal DropCollectionMessage Message;
 
        public DropCollectionMongoRequest(DropCollectionMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Message = message;
            RequestNumber = message.Header.RequestNumber;
            ParseAsync = DropCollectionCallbackHolder.DropCollectionParseAsync;
            WriteAsync = DropCollectionCallbackHolder.WriteAsync;
        }
    }

    internal class CreateCollectionMongoRequest : MongoRequestBase
    {
        internal CreateCollectionMessage Message;

        public CreateCollectionMongoRequest(CreateCollectionMessage message, ManualResetValueTaskSource<IParserResult> completionSource) : base(completionSource)
        {
            Message = message;
            RequestNumber = message.Header.RequestNumber;
            ParseAsync = CreateCollectionCallbackHolder.CreateCollectionParseAsync;
            WriteAsync = CreateCollectionCallbackHolder.WriteAsync;
        }
    }
}
