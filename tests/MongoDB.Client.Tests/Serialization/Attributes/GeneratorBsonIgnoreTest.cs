using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Attributes
{
    [BsonSerializable]
    public partial class BsonIgoreIgnoreTestModel
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
            var model = new BsonIgoreIgnoreTestModel { A = 1, B = 2 };
            var result = await RoundTripAsync(model);

            Assert.True(result.A == 0);
            Assert.True(result.B == model.B);
        }
    }
}
