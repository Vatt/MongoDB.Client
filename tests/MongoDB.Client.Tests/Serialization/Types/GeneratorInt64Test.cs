using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    [BsonSerializable(GeneratorMode.ConstructorParameters)]
    public partial class Int64Model : GeneratorTypeTestModelBase<long, long?>, IEquatable<Int64Model>
    {
        public Int64Model(
            long property,
            long? nullableProperty,
            long? alwaysNullProperty,
            List<long> listProperty,
            List<long>? nullableListProperty,
            List<long>? alwaysNullListProperty,
            List<long?> listWithNullableTypeArgumentProperty,
            List<long?>? nullableListWithNullableTypeArgumentProperty,
            List<long?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, long> dictionaryProperty,
            Dictionary<string, long>? nullableDictionaryProperty,
            Dictionary<string, long>? alwaysNullDictionaryProperty,
            Dictionary<string, long?> dictionaryWithNullableTypeArgument,
            Dictionary<string, long?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, long?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonType.Int64;
            DictionaryBsonType = BsonType.Int64;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static Int64Model Create()
        {
            return new Int64Model(
                42, 42, null,
                new() { 42, 42 }, new() { 42, 42 }, null,
                new() { 42, null }, new() { 42, null }, null,
                new() { { "42", 42 }, { "24", 24 } }, new() { { "42", 42 }, { "24", 24 } }, null,
                new() { { "42", 42 }, { "24", 24 } }, new() { { "42", 42 }, { "24", null } }, null);
        }

        public bool Equals(Int64Model other)
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
            return Equals(obj as Int64Model);
        }
    }

    [BsonSerializable]
    public partial record Int64AsStringModel(string Value = "123");

    [BsonSerializable]
    public partial record Int64AsInt32Model(int Value = 123);

    [BsonSerializable]
    public partial record Int64AsInt64Model(long Value = 123);
    public class GeneratorInt64Test : SerializationTestBase
    {
        [Fact]
        public async Task Int64Test()
        {
            var model = Int64Model.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
            model.Equals(bson);
        }
        [Fact]
        public async Task Int64AsStringTest()
        {
            var model = new Int64AsStringModel();
            var result = await RoundTripAsync<Int64AsStringModel, Int64AsInt64Model>(model);
            Assert.Equal(new Int64AsInt64Model(), result);
        }

        [Fact]
        public async Task Int64AsInt32Test()
        {
            var model = new Int64AsInt32Model();
            var result = await RoundTripAsync<Int64AsInt32Model, Int64AsInt64Model>(model);
            Assert.Equal(new Int64AsInt64Model(), result);
        }
    }
}
