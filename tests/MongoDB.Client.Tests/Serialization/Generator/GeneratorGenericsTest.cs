using MongoDB.Client.Bson;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Types;
using Xunit;
using GenericTypedef = MongoDB.Client.Tests.Serialization.Generator.GeneratorGenericModel<
    string, int, double, bool, MongoDB.Client.Bson.Document.BsonDocument,
    MongoDB.Client.Bson.Document.BsonObjectId, MongoDB.Client.Bson.Document.BsonTimestamp, System.Guid, System.DateTimeOffset, long,
    MongoDB.Client.Tests.Serialization.Types.BsonObjectIdModel, MongoDB.Client.Tests.Serialization.Types.GuidModel,
    MongoDB.Client.Tests.Serialization.Types.BooleanModel, MongoDB.Client.Tests.Serialization.Types.DoubleModel,
    MongoDB.Client.Tests.Serialization.Types.BsonTimestampModel>;

namespace MongoDB.Client.Tests.Serialization.Generator
{
    [BsonSerializable]
    public partial class GeneratorGenericModel<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IEquatable<GeneratorGenericModel<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>>
    {
        protected BsonType BsonType;
        protected BsonType DictionaryBsonType;

        public T0 Property { get; }
        public T1 NullableProperty { get; }
        public T2 AlwaysNullProperty { get; }
        public List<T3> ListProperty { get; }
        public List<T4>? NullableListProperty { get; }
        public List<T5>? AlwaysNullListProperty { get; }
        public List<T6> ListWithNullableTypeArgumentProperty { get; }
        public List<T7>? NullableListWithNullableTypeArgumentProperty { get; }
        public List<T8>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        public Dictionary<string, T9> DictionaryProperty { get; }
        public Dictionary<string, T10>? NullableDictionaryProperty { get; }
        public Dictionary<string, T11>? AlwaysNullDictionaryProperty { get; }
        public Dictionary<string, T12> DictionaryWithNullableTypeArgument { get; }
        public Dictionary<string, T13>? NullableDictionaryWithNullableTypeArgument { get; }
        public Dictionary<string, T14>? AlwaysNullDictionaryWithNullableTypeArgument { get; }
        public GeneratorGenericModel(
            T0 property,
            T1 nullableProperty,
            T2 alwaysNullProperty,
            List<T3> listProperty,
            List<T4>? nullableListProperty,
            List<T5>? alwaysNullListProperty,
            List<T6> listWithNullableTypeArgumentProperty,
            List<T7>? nullableListWithNullableTypeArgumentProperty,
            List<T8>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, T9> dictionaryProperty,
            Dictionary<string, T10>? nullableDictionaryProperty,
            Dictionary<string, T11>? alwaysNullDictionaryProperty,
            Dictionary<string, T12> dictionaryWithNullableTypeArgument,
            Dictionary<string, T13>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, T14>? alwaysNullDictionaryWithNullableTypeArgument)
        {
            Property = property;
            NullableProperty = nullableProperty;
            AlwaysNullProperty = alwaysNullProperty;
            ListProperty = listProperty;
            NullableListProperty = nullableListProperty;
            AlwaysNullListProperty = alwaysNullListProperty;
            ListWithNullableTypeArgumentProperty = listWithNullableTypeArgumentProperty;
            NullableListWithNullableTypeArgumentProperty = nullableListWithNullableTypeArgumentProperty;
            AlwaysNullListWithNullableTypeArgumentProperty = alwaysNullListWithNullableTypeArgumentProperty;
            DictionaryProperty = dictionaryProperty;
            NullableDictionaryProperty = nullableDictionaryProperty;
            AlwaysNullDictionaryProperty = alwaysNullDictionaryProperty;
            DictionaryWithNullableTypeArgument = dictionaryWithNullableTypeArgument;
            NullableDictionaryWithNullableTypeArgument = nullableDictionaryWithNullableTypeArgument;
            AlwaysNullDictionaryWithNullableTypeArgument = alwaysNullDictionaryWithNullableTypeArgument;
        }
        public static GenericTypedef Create()
        {
            return new GenericTypedef(
                "42", 42, 42,
                new() { true, false }, new() { new BsonDocument("42", "42"), new BsonDocument("42", "42") }, null,
                new() { new BsonTimestamp(1232312), new BsonTimestamp(1232312) }, new() { Guid.NewGuid(), Guid.NewGuid() }, null,
                new() { { "0", 42 }, { "1", 42 } }, new() { { "2", BsonObjectIdModel.Create() }, { "3", BsonObjectIdModel.Create() } }, null,
                new() { { "4", BooleanModel.Create() }, { "5", BooleanModel.Create() } }, new() { { "6", DoubleModel.Create() }, { "7", null } }, null);
        }

        public bool Equals(GeneratorGenericModel<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> other)
        {
            return other != null &&
                   BsonType == other.BsonType &&
                   DictionaryBsonType == other.DictionaryBsonType &&
                   Property.Equals(other.Property) &&
                   NullableProperty.Equals(other.NullableProperty) &&
                   AlwaysNullProperty.Equals(other.AlwaysNullProperty) &&
                   ListProperty!.SequenceEqual(other.ListProperty!) &&
                   NullableListProperty!.SequenceEqual(other.NullableListProperty!) &&
                   AlwaysNullListProperty is null && other.AlwaysNullListProperty is null &&
                   ListWithNullableTypeArgumentProperty!.SequenceEqual(other.ListWithNullableTypeArgumentProperty!) &&
                   NullableListWithNullableTypeArgumentProperty!.SequenceEqual(other.NullableListWithNullableTypeArgumentProperty!) &&
                   AlwaysNullListWithNullableTypeArgumentProperty is null && other.AlwaysNullListWithNullableTypeArgumentProperty is null &&
                   DictionaryProperty!.SequenceEqual(other.DictionaryProperty!) &&
                   NullableDictionaryProperty!.SequenceEqual(other.NullableDictionaryProperty!) &&
                   AlwaysNullDictionaryProperty is null && other.AlwaysNullDictionaryProperty is null &&
                   DictionaryWithNullableTypeArgument!.SequenceEqual(other.DictionaryWithNullableTypeArgument!) &&
                   NullableDictionaryWithNullableTypeArgument!.SequenceEqual(other.NullableDictionaryWithNullableTypeArgument!) &&
                   AlwaysNullDictionaryWithNullableTypeArgument is null && other.AlwaysNullDictionaryWithNullableTypeArgument is null;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GeneratorGenericModel<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
    [BsonSerializable(GeneratorMode.ConstructorParameters)]
    public partial class GenericTypeTestModel : GeneratorTypeTestModelBase<GenericTypedef, GenericTypedef?>, IEquatable<GenericTypeTestModel>
    {
        public GenericTypeTestModel(
            GenericTypedef property,
            GenericTypedef? nullableProperty,
            GenericTypedef? alwaysNullProperty,
            List<GenericTypedef> listProperty,
            List<GenericTypedef>? nullableListProperty,
            List<GenericTypedef>? alwaysNullListProperty,
            List<GenericTypedef?> listWithNullableTypeArgumentProperty,
            List<GenericTypedef?>? nullableListWithNullableTypeArgumentProperty,
            List<GenericTypedef?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, GenericTypedef> dictionaryProperty,
            Dictionary<string, GenericTypedef>? nullableDictionaryProperty,
            Dictionary<string, GenericTypedef>? alwaysNullDictionaryProperty,
            Dictionary<string, GenericTypedef?> dictionaryWithNullableTypeArgument,
            Dictionary<string, GenericTypedef?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, GenericTypedef?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonType.BinaryData;
            DictionaryBsonType = BsonType.BinaryData;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static GenericTypeTestModel Create()
        {
            var value = GenericTypedef.Create();
            return new GenericTypeTestModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(GenericTypeTestModel other)
        {
            return other != null &&
                   BsonType == other.BsonType &&
                   DictionaryBsonType == other.DictionaryBsonType &&
                   Property.Equals(other.Property) &&
                   NullableProperty.Equals(other.NullableProperty) &&
                   AlwaysNullProperty is null && other.AlwaysNullProperty is null &&
                   ListProperty!.SequenceEqual(other.ListProperty!) &&
                   NullableListProperty!.SequenceEqual(other.NullableListProperty!) &&
                   AlwaysNullListProperty is null && other.AlwaysNullListProperty is null &&
                   ListWithNullableTypeArgumentProperty!.SequenceEqual(other.ListWithNullableTypeArgumentProperty!) &&
                   NullableListWithNullableTypeArgumentProperty!.SequenceEqual(other.NullableListWithNullableTypeArgumentProperty!) &&
                   AlwaysNullListWithNullableTypeArgumentProperty is null && other.AlwaysNullListWithNullableTypeArgumentProperty is null &&
                   DictionaryProperty!.SequenceEqual(other.DictionaryProperty!) &&
                   NullableDictionaryProperty!.SequenceEqual(other.NullableDictionaryProperty!) &&
                   AlwaysNullDictionaryProperty is null && other.AlwaysNullDictionaryProperty is null &&
                   DictionaryWithNullableTypeArgument!.SequenceEqual(other.DictionaryWithNullableTypeArgument!) &&
                   NullableDictionaryWithNullableTypeArgument!.SequenceEqual(other.NullableDictionaryWithNullableTypeArgument!) &&
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
            return Equals(obj as GuidModel);
        }
    }
    public class GeneratorGenericsTest : SerializationTestBase
    {
        [Fact]
        public async Task GenericsTest()
        {
            var model = GenericTypedef.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task GenericsTypeTest()
        {
            var model = GenericTypeTestModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
    }
}
