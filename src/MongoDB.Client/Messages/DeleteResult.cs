using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.DisableTypeChecks)]
    public partial class DeleteResult : IParserResult
    {
        [BsonElement("n")]
        public int N { get; }

        [BsonElement("ok")]
        public double Ok { get; }

        public DeleteResult(int N, double Ok)
        {
            this.N = N;
            this.Ok = Ok;
        }
    }
}
