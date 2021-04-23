using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Attributes
{
    [BsonSerializable]
    public partial class BsonIgoreTestModel
    {
        [BsonIgnore]
        public int A;
        public int B;
    }
    public class GeneratorBsonIgnoreTest : SerializationTestBase
    {
        [Fact]
        public async Task BsonIgnoreTest()
        {
            var model = new BsonIgoreTestModel { A = 1, B = 2 };
            var result = await RoundTripAsync(model);

            Assert.True(result.A == 0);
            Assert.True(result.B == model.B);
        }
    }
}
