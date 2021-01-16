using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Serialization.TestModels;
using System;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedRecordTest : BaseSerialization
    {
        [Fact]
        public async Task RecordTest()
        {
            var model = new RecordModel0(42, 42, 42, "42", Guid.NewGuid());
            model.Document = new BsonDocument("42", "42");
            var result = await RoundTripAsync(model);

            Assert.Equal(result, model);
        }
    }
}