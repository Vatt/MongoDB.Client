using MongoDB.Client.Bson.Document;

namespace MongoDB.Client.Tests.Models
{
    public interface IIdentified
    {
        public BsonObjectId Id { get; }
    }
}
