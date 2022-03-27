using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Messages
{
    [BsonSerializable(GeneratorMode.SkipWriteBson)]
    public partial class Upserted
    {
        [BsonElement("index")]
        public int Index { get; }

        [BsonId]
        public BsonObjectId Id { get; }
        public Upserted(int index, BsonObjectId id)
        {
            Index = index;
            Id = id;
        }
    }
    [BsonSerializable(GeneratorMode.SkipWriteBson)]
    public partial class UpdateResult : IParserResult
    {
        [BsonElement("ok")]
        public double Ok { get; }
        
        [BsonElement("n")]
        public int N { get; }
        
        [BsonElement("nModified")]
        public int Modified { get; }

        [BsonElement("upserted")]
        public List<Upserted>? Upserted { get; }

        [BsonElement("$clusterTime")]
        public MongoClusterTime? ClusterTime { get; }
        
        [BsonElement("errmsg")]
        public string? ErrorMessage { get; }
        public UpdateResult( double ok, int n, int modified, List<Upserted> upserted, MongoClusterTime clusterTime, string errorMessage)
        {
            Upserted = upserted;
            Modified = modified;
            Ok = ok;
            N = n;
            ClusterTime = clusterTime;
            ErrorMessage = errorMessage;
        }
    }
}
