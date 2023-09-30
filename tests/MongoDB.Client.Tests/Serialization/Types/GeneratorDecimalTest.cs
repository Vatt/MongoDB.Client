using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    [BsonSerializable(GeneratorMode.ConstructorParameters)]
    public partial class DecimalModel : GeneratorTypeTestModelBase<decimal, decimal?>, IEquatable<DecimalModel>
    {
        public DecimalModel(
            decimal property,
            decimal? nullableProperty,
            decimal? alwaysNullProperty,
            List<decimal> listProperty,
            List<decimal>? nullableListProperty,
            List<decimal>? alwaysNullListProperty,
            List<decimal?> listWithNullableTypeArgumentProperty,
            List<decimal?>? nullableListWithNullableTypeArgumentProperty,
            List<decimal?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, decimal> dictionaryProperty,
            Dictionary<string, decimal>? nullableDictionaryProperty,
            Dictionary<string, decimal>? alwaysNullDictionaryProperty,
            Dictionary<string, decimal?> dictionaryWithNullableTypeArgument,
            Dictionary<string, decimal?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, decimal?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonType.Decimal;
            DictionaryBsonType = BsonType.Decimal;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static DecimalModel Create()
        {
            return new DecimalModel(
                (decimal)42.42, (decimal)42.42, null,
                new() { (decimal)42.42, (decimal)42.42 }, new() { (decimal)42.42, (decimal)42.42 }, null,
                new() { (decimal)42.42, null }, new() { (decimal)42.42, null }, null,
                new() { { "42", (decimal)24.24 }, { "24", (decimal)24.24 } }, new() { { "42", (decimal)42.42 }, { "24", (decimal)24.24 } }, null,
                new() { { "42", (decimal)24.24 }, { "24", (decimal)24.24 } }, new() { { "42", (decimal)42.42 }, { "24", null } }, null);
        }

        public bool Equals(DecimalModel other)
        {
            return other != null &&
                   BsonType == other.BsonType &&
                   DictionaryBsonType == other.DictionaryBsonType &&
                   Property == other.Property &&
                   NullableProperty == other.NullableProperty &&
                   AlwaysNullProperty == other.AlwaysNullProperty &&
                   ListProperty.SequenceEqual(other.ListProperty) &&
                   NullableListProperty.SequenceEqual(other.NullableListProperty) &&
                   AlwaysNullListProperty is null && other.AlwaysNullListProperty is null &&
                   ListWithNullableTypeArgumentProperty.SequenceEqual(other.ListWithNullableTypeArgumentProperty) &&
                   NullableListWithNullableTypeArgumentProperty.SequenceEqual(other.NullableListWithNullableTypeArgumentProperty) &&
                   AlwaysNullListWithNullableTypeArgumentProperty is null && other.AlwaysNullListWithNullableTypeArgumentProperty is null &&
                   DictionaryProperty.SequenceEqual(other.DictionaryProperty) &&
                   NullableDictionaryProperty.SequenceEqual(other.NullableDictionaryProperty) &&
                   AlwaysNullDictionaryProperty is null && other.AlwaysNullDictionaryProperty is null &&
                   DictionaryWithNullableTypeArgument.SequenceEqual(other.DictionaryWithNullableTypeArgument) &&
                   NullableDictionaryWithNullableTypeArgument.SequenceEqual(other.NullableDictionaryWithNullableTypeArgument) &&
                   AlwaysNullDictionaryWithNullableTypeArgument is null && other.AlwaysNullDictionaryWithNullableTypeArgument is null;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(BsonType);
            hash.Add(DictionaryBsonType);
            hash.Add(Property);
            hash.Add(NullableProperty);
            hash.Add(AlwaysNullProperty);
            hash.Add(ListProperty);
            hash.Add(NullableListProperty);
            hash.Add(AlwaysNullListProperty);
            hash.Add(ListWithNullableTypeArgumentProperty);
            hash.Add(NullableListWithNullableTypeArgumentProperty);
            hash.Add(AlwaysNullListWithNullableTypeArgumentProperty);
            hash.Add(DictionaryProperty);
            hash.Add(NullableDictionaryProperty);
            hash.Add(AlwaysNullDictionaryProperty);
            hash.Add(DictionaryWithNullableTypeArgument);
            hash.Add(NullableDictionaryWithNullableTypeArgument);
            hash.Add(AlwaysNullDictionaryWithNullableTypeArgument);
            return hash.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DecimalModel);
        }
    }

    [BsonSerializable]
    public partial record DecimalAsStringModel(string Value = "123");

    [BsonSerializable]
    public partial record DecimalAsInt32Model(int Value = 123);

    [BsonSerializable]
    public partial record DecimalAsInt64Model(long Value = 123);

    [BsonSerializable]
    public partial record DecimalAsDecimalModel(decimal Value = 123);

    public class GeneratorDecimalTest : SerializationTestBase
    {
        [Fact]
        public async Task DecimalTest()
        {
            var model = DecimalModel.Create();
            var doubleModel = DoubleModel.Create();
            var result1 = await RoundTripAsync(model);
            Assert.Equal(model, result1);
            var result2 = await RoundTripAsync<DoubleModel, DecimalModel>(doubleModel);
            Assert.Equal(model, result2);
        }

        [Fact]
        public async Task DecimalAsStringTest()
        {
            var model = new DecimalAsStringModel();
            var result = await RoundTripAsync<DecimalAsStringModel, DecimalAsDecimalModel>(model);
            Assert.Equal(new DecimalAsDecimalModel(), result);
        }

        [Fact]
        public async Task DecimalAsInt32Test()
        {
            var model = new DecimalAsInt32Model();
            var result = await RoundTripAsync<DecimalAsInt32Model, DecimalAsDecimalModel>(model);
            Assert.Equal(new DecimalAsDecimalModel(), result);
        }
        
        [Fact]
        public async Task DecimalAsInt64Test()
        {
            var model = new DecimalAsInt64Model();
            var result = await RoundTripAsync<DecimalAsInt64Model, DecimalAsDecimalModel>(model);
            Assert.Equal(new DecimalAsDecimalModel(), result);
        }
    }
}
