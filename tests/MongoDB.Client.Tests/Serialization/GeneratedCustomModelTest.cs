using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedCustomModelTest : BaseSerialization
    {
        [Fact]
        public async Task CustomModelTest()
        {
            var model = new ModelWithCustom("CustomModelTest", new CustomModel(42, 42, 42));

            var result = await RoundTripAsync(model);
            Assert.Equal(result, model);
        }
    }
}
