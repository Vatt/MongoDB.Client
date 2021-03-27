using System;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization
{
    public class GeneratedTwoModelsTest : BaseSerialization
    {
        [Fact]
        public async Task TwoModelsTest()
        {
            var model = new Model1 { MyyProperty = "MyProperty", MyValue = "MyValue", MyyAndVal = "AndVal", MyyOtherVal = "OtherVal", MyySomeValue = "SomeValue", A = "A" };
            var result = await RoundTripAsync<Model1, Model2>(model);

            Assert.True(model.Equals(result));
        }
    }

    [BsonSerializable]
    public partial class Model1 : IEquatable<Model1>, IEquatable<Model2>
    {
        public string MyyProperty { get; set; }
        public string MyValue { get; set; }
        public string MyyOtherVal { get; set; }
        public string MyyAndVal { get; set; }
        public string MyySomeValue { get; set; }
        public string My { get; set; }
        public string A { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Model1 model && Equals(model);

        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MyyProperty, MyValue, MyyOtherVal, MyyAndVal, MyySomeValue, A, My);
        }

        public bool Equals(Model2 other)
        {
            return other is not null &&
                   MyyProperty == other.MyyProperty &&
                   MyValue == other.MyValue &&
                   MyyOtherVal == other.MyyOtherVal &&
                   MyyAndVal == other.MyyAndVal &&
                   MyySomeValue == other.MyySomeValue;
        }

        public bool Equals(Model1 other)
        {
            return other is not null &&
                   MyyProperty == other.MyyProperty &&
                   MyValue == other.MyValue &&
                   MyyOtherVal == other.MyyOtherVal &&
                   MyyAndVal == other.MyyAndVal &&
                   MyySomeValue == other.MyySomeValue &&
                   A == other.A &&
                   My == other.My;
        }
    }

    [BsonSerializable]
    public partial class Model2 : IEquatable<Model1>, IEquatable<Model2>
    {
        public string MyyProperty { get; set; }
        public string MyValue { get; set; }
        public string MyyOtherVal { get; set; }
        public string MyyAndVal { get; set; }
        public string MyySomeValue { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Model2 model && Equals(model);
        }

        public bool Equals(Model2 other)
        {
            return other is not null &&
                   MyyProperty == other.MyyProperty &&
                   MyValue == other.MyValue &&
                   MyyOtherVal == other.MyyOtherVal &&
                   MyyAndVal == other.MyyAndVal &&
                   MyySomeValue == other.MyySomeValue;
        }

        public bool Equals(Model1 other)
        {
            return other is not null &&
                   MyyProperty == other.MyyProperty &&
                   MyValue == other.MyValue &&
                   MyyOtherVal == other.MyyOtherVal &&
                   MyyAndVal == other.MyyAndVal &&
                   MyySomeValue == other.MyySomeValue;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MyyProperty, MyValue);
        }
    }
}
