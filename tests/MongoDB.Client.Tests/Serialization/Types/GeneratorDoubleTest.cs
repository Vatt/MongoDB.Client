using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    [BsonSerializable(GeneratorMode.ConstructorOnlyParameters)]
    public partial class DoubleModel : GeneratorTypeTestModelBase<double, double?>, IEquatable<DoubleModel>
    {
        public DoubleModel(
            double property,
            double? nullableProperty,
            double? alwaysNullProperty,
            List<double> listProperty,
            List<double>? nullableListProperty,
            List<double>? alwaysNullListProperty,
            List<double?> listWithNullableTypeArgumentProperty,
            List<double?>? nullableListWithNullableTypeArgumentProperty,
            List<double?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, double> dictionaryProperty,
            Dictionary<string, double>? nullableDictionaryProperty,
            Dictionary<string, double>? alwaysNullDictionaryProperty,
            Dictionary<string, double?> dictionaryWithNullableTypeArgument,
            Dictionary<string, double?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, double?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.Double;
            DictionaryBsonType = BsonElementType.Double;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static DoubleModel Create()
        {
            return new DoubleModel(
                42.42, 42.42, null,
                new() { 42.42, 42.42 }, new() { 42.42, 42.42 }, null,
                new() { 42.42, null }, new() { 42.42, null }, null,
                new() { { "42", 24.24 }, { "24", 24.24 } }, new() { { "42", 42.42 }, { "24", 24.24 } }, null,
                new() { { "42", 24.24 }, { "24", 24.24 } }, new() { { "42", 42.42 }, { "24", null } }, null);
        }

        public bool Equals(DoubleModel other)
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
            return Equals(obj as DoubleModel);
        }
    }


    public class GeneratorDoubleTest : SerializationTestBase
    {
        [Fact]
        public async Task DoubleTest()
        {
            var model = DoubleModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
