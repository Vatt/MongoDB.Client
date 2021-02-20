using MongoDB.Client.Bson.Document;
using MongoDB.Client.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class NullableTest : BaseSerialization
    {
        [Fact]
        public async Task  IntNullableTest()
        {
            var model = new IntNullable() { Prop = 42, Field = null };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);

        }
        [Fact]
        public async Task DoubleNullableTest()
        {
            var model = new DoubleNullable() { Prop = 42, Field = null };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task LongNullableTest()
        {
            var model = new LongNullable() { Prop = 42, Field = null };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StringNullableTest()
        {
            var model = new StringNullable() { Prop = "42", Field = "42" };
            var result = await RoundTripAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task DateTimeOffsetNullableTest()
        {
            var model = new DateTimeOffsetNullable() { Prop = new DateTimeOffset(2021, 01, 01, 5, 30, 0,TimeSpan.Zero), Field = null };
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
    }
}
