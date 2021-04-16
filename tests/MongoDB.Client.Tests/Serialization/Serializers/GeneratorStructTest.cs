using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Serializers
{
    [BsonSerializable]
    public partial struct GeneratorStructWithManyCtorsTestModel : IEquatable<GeneratorStructWithManyCtorsTestModel>
    {
        public int Value;

        [BsonConstructor]
        public GeneratorStructWithManyCtorsTestModel(int value)
        {
            Value = value;
        }
        public GeneratorStructWithManyCtorsTestModel(string value)
        {
            Value = int.Parse(value);
        }
        public override bool Equals(object obj)
        {
            return obj is GeneratorStructWithManyCtorsTestModel model && Equals(model);
        }

        public bool Equals(GeneratorStructWithManyCtorsTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    [BsonSerializable]
    public partial struct GeneratorStructTestModel : IEquatable<GeneratorStructTestModel>
    {
        public int Value;

        public GeneratorStructTestModel(int value)
        {
            Value = value;
        }
        public override bool Equals(object obj)
        {
            return obj is GeneratorStructTestModel model && Equals(model);
        }

        public bool Equals(GeneratorStructTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    [BsonSerializable]
    public partial struct GeneratorStructWithFreeFieldTestModel : IEquatable<GeneratorStructWithFreeFieldTestModel>
    {
        public readonly int Value;
        public int Value1;
        //TODO: без атрибута констурктор не мапится
        [BsonConstructor]
        public GeneratorStructWithFreeFieldTestModel(int value)
        {
            Value = value;
            Value1 = default;
        }
        public override bool Equals(object obj)
        {
            return obj is GeneratorStructWithFreeFieldTestModel model && Equals(model);
        }

        public bool Equals(GeneratorStructWithFreeFieldTestModel other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }
    }
    [BsonSerializable]
    public readonly partial struct GeneratorReadonlyStructTestModel : IEquatable<GeneratorReadonlyStructTestModel>
    {
        public readonly int Value;
        public GeneratorReadonlyStructTestModel(int value)
        {
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is GeneratorReadonlyStructTestModel model && Equals(model);
        }

        public bool Equals(GeneratorReadonlyStructTestModel other)
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
            var model = new GeneratorStructWithManyCtorsTestModel(2);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StrcutTest()
        {
            var model = new GeneratorStructTestModel(2);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task StrcutWithFreeFieldTest()
        {
            var model = new GeneratorStructWithFreeFieldTestModel(2){ Value1 = 1 };
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ReadonlyStrcutTest()
        {
            var model = new GeneratorReadonlyStructTestModel(2);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);

        }
    }

}
