using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Serializers
{
    [BsonSerializable]
    public partial struct StructWithManyCtorsTestModel : IEquatable<StructWithManyCtorsTestModel>
    {
        public int Value;

        [BsonConstructor]
        public StructWithManyCtorsTestModel(int value)
        {
            Value = value;
        }
        public StructWithManyCtorsTestModel(string value)
        {
            Value = int.Parse(value);
        }
        public override bool Equals(object obj)
        {
            return obj is StructWithManyCtorsTestModel model && Equals(model);
        }

        public bool Equals(StructWithManyCtorsTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    [BsonSerializable]
    public partial struct StructTestModel : IEquatable<StructTestModel>
    {
        public int Value;

        public StructTestModel(int value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            return obj is StructTestModel model && Equals(model);
        }

        public bool Equals(StructTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    [BsonSerializable]
    public partial struct StructWithFreeFieldTestModel : IEquatable<StructWithFreeFieldTestModel>
    {
        public readonly int Value;
        public int Value1;
        //TODO: без атрибута констурктор не мапится
        [BsonConstructor]
        public StructWithFreeFieldTestModel(int value)
        {
            Value = value;
            Value1 = default;
        }
        public override bool Equals(object obj)
        {
            return obj is StructWithFreeFieldTestModel model && Equals(model);
        }

        public bool Equals(StructWithFreeFieldTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    [BsonSerializable]
    public readonly partial struct ReadonlyStructTestModel : IEquatable<ReadonlyStructTestModel>
    {
        public readonly int Value;
        public ReadonlyStructTestModel(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is ReadonlyStructTestModel model && Equals(model);
        }

        public bool Equals(ReadonlyStructTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    public class GeneratorStructTests : SerializationTestBase
    {
        [Fact]
        public async Task StructWithManyCtorsTest()
        {
            var model = new StructWithManyCtorsTestModel(2);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StrcutTest()
        {
            var model = new StructTestModel(2);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StrcutWithFreeFieldTest()
        {
            var model = new StructWithFreeFieldTestModel(2) { Value1 = 1 };
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ReadonlyStrcutTest()
        {
            var model = new ReadonlyStructTestModel(2);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);

        }
    }

}
