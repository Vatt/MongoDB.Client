using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class ClusterTimeTest : BaseSerialization
    {
        [Fact]
        public async Task MongoClusterTimeTest()
        {
            var sign = new MongoSignature(new byte[12] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11}, 42);
            var clusterTime = new MongoClusterTime(new BsonTimestamp(6936540292354932737), sign);
            var result = await RoundTripAsync(clusterTime);
            Assert.Equal(clusterTime, result);
        }
    }
}