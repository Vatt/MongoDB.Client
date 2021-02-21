using MongoDB.Bson;
using BsonObjectId = MongoDB.Client.Bson.Document.BsonObjectId;

namespace MongoDB.Client.Tests.Models
{
    public interface IIdentified
    {
        public BsonObjectId Id { get; }
        public ObjectId OldId { get; }
    }
}
