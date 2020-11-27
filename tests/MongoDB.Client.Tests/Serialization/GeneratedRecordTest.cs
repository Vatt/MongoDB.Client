using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization;
using MongoDB.Client.Tests.Serialization.TestModels;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedRecordTest : BaseSerialization
    {
        //[Fact]
        //public async Task RecordTest()
        //{
        //    var model = new RecordModel0(42, 42, 42, "42", Guid.NewGuid());
        //    model.Document = new BsonDocument("42", "42");
        //    SerializersMap.TryGetSerializer<RecordModel0>(out var serializer);
        //    var result = await RoundTripAsync(model, serializer);

        //    Assert.Equal(result, model);
        //}
        //[Fact]
        //public async Task RecordWithConstructorTest()
        //{ 
        //    var model = new RecordModel2(1);
        //    SerializersMap.TryGetSerializer<RecordModel2>(out var serializer);
        //    var result = await RoundTripAsync(model, serializer);

        //    Assert.Equal(result, model);
        //}
    }
}