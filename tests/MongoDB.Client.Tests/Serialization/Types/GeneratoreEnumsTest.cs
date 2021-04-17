using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    public enum TestEnum
    {
        One,
        Two,
        Three
    }
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class StringEnumModel : IEquatable<StringEnumModel>
    {
        protected BsonElementType BsonType;
        protected BsonElementType DictionaryBsonType;
        [BsonEnum(EnumRepresentation.String)]
        public TestEnum Property { get; }
        [BsonEnum(EnumRepresentation.String)]
        public TestEnum? NullableProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public TestEnum? AlwaysNullProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public List<TestEnum> ListProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public List<TestEnum>? NullableListProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public List<TestEnum>? AlwaysNullListProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public List<TestEnum?> ListWithNullableTypeArgumentProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public List<TestEnum?>? NullableListWithNullableTypeArgumentProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public List<TestEnum?>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public Dictionary<string, TestEnum> DictionaryProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public Dictionary<string, TestEnum>? NullableDictionaryProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public Dictionary<string, TestEnum>? AlwaysNullDictionaryProperty { get; }
        [BsonEnum(EnumRepresentation.String)]
        public Dictionary<string, TestEnum?> DictionaryWithNullableTypeArgument { get; }
        [BsonEnum(EnumRepresentation.String)]
        public Dictionary<string, TestEnum?>? NullableDictionaryWithNullableTypeArgument { get; }
        [BsonEnum(EnumRepresentation.String)]
        public Dictionary<string, TestEnum?>? AlwaysNullDictionaryWithNullableTypeArgument { get; }
        public StringEnumModel(
            TestEnum property,
            TestEnum? nullableProperty,
            TestEnum? alwaysNullProperty,
            List<TestEnum> listProperty,
            List<TestEnum>? nullableListProperty,
            List<TestEnum>? alwaysNullListProperty,
            List<TestEnum?> listWithNullableTypeArgumentProperty,
            List<TestEnum?>? nullableListWithNullableTypeArgumentProperty,
            List<TestEnum?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, TestEnum> dictionaryProperty,
            Dictionary<string, TestEnum>? nullableDictionaryProperty,
            Dictionary<string, TestEnum>? alwaysNullDictionaryProperty,
            Dictionary<string, TestEnum?> dictionaryWithNullableTypeArgument,
            Dictionary<string, TestEnum?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, TestEnum?>? alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.BinaryData;
            DictionaryBsonType = BsonElementType.BinaryData;
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
        public bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static StringEnumModel Create()
        {
            var value = TestEnum.Three;
            return new StringEnumModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(StringEnumModel other)
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

        public override bool Equals(object obj)
        {
            return Equals(obj as StringEnumModel);
        }
    }
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class Int64EnumModel : IEquatable<Int64EnumModel>
    {
        protected BsonElementType BsonType;
        protected BsonElementType DictionaryBsonType;
        [BsonEnum(EnumRepresentation.Int64)]
        public TestEnum Property { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public TestEnum? NullableProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public TestEnum? AlwaysNullProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public List<TestEnum> ListProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public List<TestEnum>? NullableListProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public List<TestEnum>? AlwaysNullListProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public List<TestEnum?> ListWithNullableTypeArgumentProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public List<TestEnum?>? NullableListWithNullableTypeArgumentProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public List<TestEnum?>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public Dictionary<string, TestEnum> DictionaryProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public Dictionary<string, TestEnum>? NullableDictionaryProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public Dictionary<string, TestEnum>? AlwaysNullDictionaryProperty { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public Dictionary<string, TestEnum?> DictionaryWithNullableTypeArgument { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public Dictionary<string, TestEnum?>? NullableDictionaryWithNullableTypeArgument { get; }
        [BsonEnum(EnumRepresentation.Int64)]
        public Dictionary<string, TestEnum?>? AlwaysNullDictionaryWithNullableTypeArgument { get; }
        public Int64EnumModel(
            TestEnum property,
            TestEnum? nullableProperty,
            TestEnum? alwaysNullProperty,
            List<TestEnum> listProperty,
            List<TestEnum>? nullableListProperty,
            List<TestEnum>? alwaysNullListProperty,
            List<TestEnum?> listWithNullableTypeArgumentProperty,
            List<TestEnum?>? nullableListWithNullableTypeArgumentProperty,
            List<TestEnum?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, TestEnum> dictionaryProperty,
            Dictionary<string, TestEnum>? nullableDictionaryProperty,
            Dictionary<string, TestEnum>? alwaysNullDictionaryProperty,
            Dictionary<string, TestEnum?> dictionaryWithNullableTypeArgument,
            Dictionary<string, TestEnum?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, TestEnum?>? alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.BinaryData;
            DictionaryBsonType = BsonElementType.BinaryData;
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
        public bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static Int64EnumModel Create()
        {
            var value = TestEnum.Two;
            return new Int64EnumModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(Int64EnumModel other)
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
            return Equals(obj as Int64EnumModel);
        }
    }
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class Int32EnumModel : GeneratorTypeTestModelBase<TestEnum, TestEnum?>, IEquatable<Int32EnumModel>
    {
        public Int32EnumModel(
            TestEnum property,
            TestEnum? nullableProperty,
            TestEnum? alwaysNullProperty,
            List<TestEnum> listProperty,
            List<TestEnum>? nullableListProperty,
            List<TestEnum>? alwaysNullListProperty,
            List<TestEnum?> listWithNullableTypeArgumentProperty,
            List<TestEnum?>? nullableListWithNullableTypeArgumentProperty,
            List<TestEnum?>? alwaysNullListWithNullableTypeArgumentProperty,
            Dictionary<string, TestEnum> dictionaryProperty,
            Dictionary<string, TestEnum>? nullableDictionaryProperty,
            Dictionary<string, TestEnum>? alwaysNullDictionaryProperty,
            Dictionary<string, TestEnum?> dictionaryWithNullableTypeArgument,
            Dictionary<string, TestEnum?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, TestEnum?>? alwaysNullDictionaryWithNullableTypeArgument)
            : base(property, nullableProperty, alwaysNullProperty,
                    listProperty, nullableListProperty, alwaysNullListProperty,
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.BinaryData;
            DictionaryBsonType = BsonElementType.BinaryData;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static Int32EnumModel Create()
        {
            var value = TestEnum.One;
            return new Int32EnumModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(Int32EnumModel other)
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
            return Equals(obj as Int32EnumModel);
        }
    }

    //TODO: для стринг енума и нулабл стринг енума генерируется два разных метода
    //TODO: Словарь коряво генерит вриты
    public class GeneratorEnumsTest : SerializationTestBase
    {
        [Fact]
        public async Task Int32EnumTest()
        {
            var model = Int32EnumModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(Int32EnumModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
        [Fact]
        public async Task Int64EnumTest()
        {
            var model = Int64EnumModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(Int64EnumModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
        [Fact]
        public async Task StringEnumTest()
        {
            var model = StringEnumModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(StringEnumModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
