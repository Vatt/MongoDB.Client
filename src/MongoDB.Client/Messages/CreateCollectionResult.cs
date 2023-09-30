using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial class CreateCollectionResult : IParserResult
    {
        [BsonElement("nIndexesWas")]
        public int NIndexesWas { get; }

        [BsonElement("ns")]
        public string Namespace { get; }

        [BsonElement("ok")]
        public double Ok { get; }

        [BsonElement("errmsg")]
        public string ErrorMessage { get; }

        [BsonElement("code")]
        public int Code { get; }

        [BsonElement("codeName")]
        public string CodeName { get; }
        public CreateCollectionResult(int NIndexesWas, string Namespace, double Ok, string ErrorMessage, int Code, string CodeName)
        {
            this.NIndexesWas = NIndexesWas;
            this.Namespace = Namespace;
            this.Ok = Ok;
            this.ErrorMessage = ErrorMessage;
            this.Code = Code;
            this.CodeName = CodeName;
        }
    }
}
