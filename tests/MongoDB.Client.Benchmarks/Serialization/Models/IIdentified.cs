using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Benchmarks.Serialization.Models
{
    public interface IIdentified
    {
        public BsonObjectId Id {get;}
    }
}
