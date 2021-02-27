using MongoDB.Client.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace MongoDB.Client.Messages
{
    [BsonSerializable]
    public partial class InsertResult : IParserResult
    {
        [BsonElement("n")]
        public int N { get; }

        [BsonElement("ok")]
        public double Ok { get; }

        [BsonElement("writeErrors")]
        public List<InsertError>? WriteErrors { get; }
        public InsertResult(int N, double Ok, List<InsertError>? WriteErrors)
        {
            this.N = N;
            this.Ok = Ok;
            this.WriteErrors = WriteErrors;
        }
    }

    [BsonSerializable]
    public partial class InsertError
    {
        [BsonElement("index")]
        public int Index { get; }

        [BsonElement("code")]
        public int Code { get; }

        [BsonElement("errmsg")]
        public string ErrorMessage { get; }
        public InsertError(int Index, int Code, string ErrorMessage)
        {
            this.Index = Index;
            this.Code = Code;
            this.ErrorMessage = ErrorMessage;
        }
    }
}