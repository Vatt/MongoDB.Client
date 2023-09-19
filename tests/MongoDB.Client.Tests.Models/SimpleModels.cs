using MongoDB.Bson;
using MongoDB.Client.Bson.Serialization.Attributes;

namespace MongoDB.Client.Tests.Models
{
    [BsonSerializable]
    public partial record IntSimpleModel(/*int Value*/) : IIdentified
    {
        public Bson.Document.BsonObjectId Id => Bson.Document.BsonObjectId.NewObjectId();

        [BsonIgnore]
        public ObjectId OldId => throw new NotImplementedException();
    }

    public class IntSimpleModelSeeder
    {
        public IEnumerable<IntSimpleModel> GenerateSeed(int count)
        {
            for (var i = 0; i < count; i++)
            {
                //yield return new IntSimpleModel(42);
                yield return new IntSimpleModel();
            }
        }
    }
}
