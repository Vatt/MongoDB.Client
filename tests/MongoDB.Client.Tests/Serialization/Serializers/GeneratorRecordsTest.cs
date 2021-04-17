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
    public partial record RecordWithParameterList(int A, string B, double C);
    [BsonSerializable]
    public partial record RecordWithParameterListAndFreeField(int A, string B, double C)
    {
        public int D { get; set; }
    }
    [BsonSerializable]
    public partial record RecordWithParameterListAndFreeFieldAndOtherCtor(int A, string B, double C)
    {
        //This Ctor was ignored because have parameter list
        [BsonConstructor]
        public RecordWithParameterListAndFreeFieldAndOtherCtor(int d) : this(d, d.ToString(), d)
        {
            D = d;
        }
        public int D { get; set; }
    }

    public class GeneratorRecordsTest : SerializationTestBase
    {
        [Fact]
        public async Task RecordWithParameterListTest()
        {
            var model = new RecordWithParameterList(42, "42", 42.24);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task RecordWithParameterListAndFreeFieldTest()
        {
            var model = new RecordWithParameterListAndFreeField(42, "42", 42.24);
            model.D = 123;
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task RecordWithParameterListAndFreeFieldAndOtherCtorTest()
        {
            var model = new RecordWithParameterListAndFreeFieldAndOtherCtor(42);
            model.D = 123;
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
    }
}
