using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    [BsonSerializable(GeneratorMode.ConstructorParameters)]
    public partial class DateTimeOffsetModel : GeneratorTypeTestModelBase<DateTimeOffset, DateTimeOffset?>, IEquatable<DateTimeOffsetModel>
    {
        public DateTimeOffsetModel(
            DateTimeOffset property,
            DateTimeOffset? nullableProperty,
            DateTimeOffset? alwaysNullProperty,
            List<DateTimeOffset> listProperty,
            List<DateTimeOffset>? nullableListProperty,
            List<DateTimeOffset>? alwaysNullListProperty,
            List<DateTimeOffset?> listWithNullableTypeArgumentProperty,
            List<DateTimeOffset?>? nullableListWithNullableTypeArgumentProperty,
            List<DateTimeOffset?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, DateTimeOffset> dictionaryProperty,
            Dictionary<string, DateTimeOffset>? nullableDictionaryProperty,
            Dictionary<string, DateTimeOffset>? alwaysNullDictionaryProperty,
            Dictionary<string, DateTimeOffset?> dictionaryWithNullableTypeArgument,
            Dictionary<string, DateTimeOffset?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, DateTimeOffset?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonType.UtcDateTime;
            DictionaryBsonType = BsonType.UtcDateTime;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static DateTimeOffsetModel Create()
        {
            var value = new DateTimeOffset(2021, 04, 11, 5, 30, 42, TimeSpan.Zero);
            return new DateTimeOffsetModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(DateTimeOffsetModel other)
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
            return Equals(obj as DateTimeOffsetModel);
        }
    }


    public class GeneratorDateTimeOffsetTest : SerializationTestBase
    {
        [Fact]
        public async Task DateTimeOffsetTest()
        {
            var model = DateTimeOffsetModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(DateTimeOffsetModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
