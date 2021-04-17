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
    //TODO: приляпать такойже хак на праймари конструктор как и с классом, один конструтор без параметр листа является праймари
    [BsonSerializable]
    public partial record RecordWithPrimaryCtor
    {
        public int A;
        public int B;
        public int C;
        public int D;
        [BsonConstructor]
        public RecordWithPrimaryCtor(int a , int b, int c, int d)
        {
            A = d;
            B = b;
            C = c;
            D = d;
        }
    }
    [BsonSerializable]
    public partial record RecordWithPrimaryCtorWithFreeField
    {
        public int A;
        public int B;
        public int C;
        public int D;
        public int E;
        [BsonConstructor]
        public RecordWithPrimaryCtorWithFreeField(int a, int b, int c, int d)
        {
            A = d;
            B = b;
            C = c;
            D = d;
        }
    }
    [BsonSerializable]
    public partial record ReadonlyRecord
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        [BsonConstructor]
        public ReadonlyRecord(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
    [BsonSerializable]
    public partial record ReadonlyRecordWithFreeField
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        public int D;
        [BsonConstructor]
        public ReadonlyRecordWithFreeField(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
    [BsonSerializable]
    public partial record GetOnlyRecord
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        [BsonConstructor]
        public GetOnlyRecord(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
    [BsonSerializable]
    public partial record GetOnlyRecordWithFreeField
    {
        public int A { get; }
        public int B { get; }
        public int C { get; }
        public int D { get; set; }
        [BsonConstructor]
        public GetOnlyRecordWithFreeField(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
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
        [Fact]
        public async Task RecordWithPrimaryCtorTest()
        {
            var model = new RecordWithPrimaryCtor(1, 2, 3, 4);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task RecordWithPrimaryCtorWithFreeFieldTest()
        {
            var model = new RecordWithPrimaryCtorWithFreeField(1, 2, 3, 4);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ReadonlyRecordTest()
        {
            var model = new ReadonlyRecord(1, 2, 3);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task ReadonlyRecordWithFreeFieldTest()
        {
            var model = new ReadonlyRecordWithFreeField(1, 2, 3);
            model.D = 42;
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GetOnlyRecordTest()
        {
            var model = new GetOnlyRecord(1, 2, 3);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GetOnlyRecordWithFreeFieldTest()
        {
            var model = new GetOnlyRecordWithFreeField(1, 2, 3);
            model.D = 42;
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }

    }
}
