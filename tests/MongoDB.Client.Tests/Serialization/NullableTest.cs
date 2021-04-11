using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class NullableTest : SerializationTestBase
    {
        [Fact]
        public async Task IntNullableTest()
        {
            var model = IntNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);

        }
        [Fact]
        public async Task DoubleNullableTest()
        {
            var model = DoubleNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task LongNullableTest()
        {
            var model = LongNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StringNullableTest()
        {
            var model = StringNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task DateTimeOffsetNullableTest()
        {
            var model = DateTimeOffsetNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GuidNullableTest()
        {
            var model = new GuidNullable() { Prop = Guid.NewGuid(), Field = Guid.NewGuid() };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task BsonObjectIdNullableTest()
        {
            var model = new BsonObjectIdNullable() { Prop = BsonObjectId.NewObjectId(), Field = BsonObjectId.NewObjectId() };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task RecordNullableTest()
        {
            var model = RecordNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StructNullableTest()
        {
            var model = StructNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ClassNullableTest()
        {
            var model = ClassNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ListNullableTest()
        {
            var model = ListNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ListElementNullableTest()
        {
            var model = ListElementNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GenericNullableTest()
        {
            var model = GenericNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GenericWithNulalbleListTest()
        {
            var model = GenericWithNullableListTest<BsonObjectId, string>.Create(BsonObjectId.NewObjectId(), BsonObjectId.NewObjectId(), BsonObjectId.NewObjectId(), "42", "43", "43");
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task OtherModelsNullableTest()
        {
            var model = OtherModelsNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task EnumNullableTest()
        {
            var model = EnumNullable.Create();
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
    }
}
