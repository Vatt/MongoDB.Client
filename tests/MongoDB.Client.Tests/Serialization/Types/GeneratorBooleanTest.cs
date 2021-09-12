using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class BooleanModel : GeneratorTypeTestModelBase<bool, bool?>, IEquatable<BooleanModel>
    {
        public BooleanModel(
            bool property,
            bool? nullableProperty,
            bool? alwaysNullProperty,
            List<bool> listProperty,
            List<bool>? nullableListProperty,
            List<bool>? alwaysNullListProperty,
            List<bool?> listWithNullableTypeArgumentProperty,
            List<bool?>? nullableListWithNullableTypeArgumentProperty,
            List<bool?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, bool> dictionaryProperty,
            Dictionary<string, bool>? nullableDictionaryProperty,
            Dictionary<string, bool>? alwaysNullDictionaryProperty,
            Dictionary<string, bool?> dictionaryWithNullableTypeArgument,
            Dictionary<string, bool?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, bool?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.Boolean;
            DictionaryBsonType = BsonElementType.Boolean;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static BooleanModel Create()
        {
            return new BooleanModel(
                true, false, null,
                new() { true, false }, new() { true, false }, null,
                new() { true, null }, new() { false, null }, null,
                new() { { "42", true }, { "24", false } }, new() { { "42", true }, { "24", false } }, null,
                new() { { "42", true }, { "24", false } }, new() { { "42", true }, { "24", null } }, null);
        }

        public bool Equals(BooleanModel other)
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
            return Equals(obj as BooleanModel);
        }
    }


    public class GeneratorBooleanTest : SerializationTestBase
    {
        [Fact]
        public async Task BooleanTest()
        {
            var model = BooleanModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(BooleanModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
