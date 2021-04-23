using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Generator
{
    public class GeneratorTwoModelsTest : SerializationTestBase
    {
        [Fact]
        public async Task TwoModelsTest()
        {
            var model = new Model1 { MyProperty = "MyProperty", MyValue = "MyValue", MyAndVal = "AndVal", MyOtherVal = "OtherVal", MySomeValue = "SomeValue", A = "A" };
            var result = await RoundTripAsync<Model1, Model2>(model);

            Assert.True(model.Equals(result));
        }
    }

    [BsonSerializable]
    public partial class Model1 : IEquatable<Model1>, IEquatable<Model2>
    {
        public string MyProperty { get; set; }
        public string MyValue { get; set; }
        public string MyOtherVal { get; set; }
        public string MyAndVal { get; set; }
        public string MySomeValue { get; set; }
        public string My { get; set; }
        public string A { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Model1 model && Equals(model);

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MyProperty, MyValue, MyOtherVal, MyAndVal, MySomeValue, A, My);
        }

        public bool Equals(Model2 other)
        {
            return other is not null &&
                   MyProperty == other.MyProperty &&
                   MyValue == other.MyValue &&
                   MyOtherVal == other.MyOtherVal &&
                   MyAndVal == other.MyAndVal &&
                   MySomeValue == other.MySomeValue;
        }

        public bool Equals(Model1 other)
        {
            return other is not null &&
                   MyProperty == other.MyProperty &&
                   MyValue == other.MyValue &&
                   MyOtherVal == other.MyOtherVal &&
                   MyAndVal == other.MyAndVal &&
                   MySomeValue == other.MySomeValue &&
                   A == other.A &&
                   My == other.My;
        }
    }

    [BsonSerializable]
    public partial class Model2 : IEquatable<Model1>, IEquatable<Model2>
    {
        public string MyProperty { get; set; }
        public string MyValue { get; set; }
        public string MyOtherVal { get; set; }
        public string MyAndVal { get; set; }
        public string MySomeValue { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Model2 model && Equals(model);
        }

        public bool Equals(Model2 other)
        {
            return other is not null &&
                   MyProperty == other.MyProperty &&
                   MyValue == other.MyValue &&
                   MyOtherVal == other.MyOtherVal &&
                   MyAndVal == other.MyAndVal &&
                   MySomeValue == other.MySomeValue;
        }

        public bool Equals(Model1 other)
        {
            return other is not null &&
                   MyProperty == other.MyProperty &&
                   MyValue == other.MyValue &&
                   MyOtherVal == other.MyOtherVal &&
                   MyAndVal == other.MyAndVal &&
                   MySomeValue == other.MySomeValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MyProperty, MyValue);
        }
    }
}
