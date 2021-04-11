using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedObjectTest : BaseSerialization
    {
        [Fact]
        public async Task ObjectTest()
        {
            var model = new ObjectModel(ByteArrayModel.Create(), 42);
            var result = await RoundTripAsync(model);
            Assert.Equal(model.ObjectProp1, result.ObjectProp1);
            var trueModelProp0 = (ByteArrayModel)model.ObjectProp0;
            var trueResultProp0 = (BsonDocument)result.ObjectProp0;
            var resultByteProp = (BsonBinaryData)(trueResultProp0["ByteProp"].Value);
            var resultMemoryByteProp = (BsonBinaryData)(trueResultProp0["MemoryByteProp"].Value);
            var resultMD5ByteProp = (BsonBinaryData)(trueResultProp0["MD5ByteProp"].Value);
            var resultMD5MemoryProp = (BsonBinaryData)(trueResultProp0["MD5MemoryProp"].Value);
            Assert.NotNull(trueModelProp0);
            Assert.NotNull(trueResultProp0);
            Assert.Equal(trueModelProp0.ByteProp, resultByteProp.Value as byte[]);
            Assert.True(trueModelProp0.MemoryByteProp.Span.SequenceEqual(resultMemoryByteProp.Value as byte[]));
            Assert.Equal(trueModelProp0.MD5ByteProp, resultMD5ByteProp.Value as byte[]);
            Assert.True(trueModelProp0.MD5MemoryProp.Value.Span.SequenceEqual(resultMD5MemoryProp.Value as byte[]));
        }
    }
}
