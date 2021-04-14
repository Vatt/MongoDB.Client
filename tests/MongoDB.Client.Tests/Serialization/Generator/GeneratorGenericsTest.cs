using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Types;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Generator
{
    [BsonSerializable]
    public partial class GeneratorGenericModel<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
    {
        protected BsonElementType BsonType;
        protected BsonElementType DictionaryBsonType;

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
        public static GeneratorGenericModel<string, int, double, bool, BsonDocument, BsonObjectId, BsonTimestamp, Guid, DateTimeOffset, long, GeneratorBsonObjectIdModel, GeneratorGuidModel, GeneratorBooleanModel, GeneratorByteArrayAsGenericModel, GeneratorBsonTimestampModel> Create()
        {
            return new GeneratorGenericModel<string, int, double, bool, BsonDocument, BsonObjectId, BsonTimestamp, Guid, DateTimeOffset, long, GeneratorBsonObjectIdModel, GeneratorGuidModel, GeneratorBooleanModel, GeneratorByteArrayAsGenericModel, GeneratorBsonTimestampModel>(
                "42", 42, 42,
                new() { true, false }, new() { new BsonDocument("42", "42"), new BsonDocument("42", "42") }, null,
                new() { new BsonTimestamp(1232312), new BsonTimestamp(1232312) }, new() { Guid.NewGuid(), Guid.NewGuid() }, null,
                new() { { "0", 42 }, { "1", 42 } }, new() { { "2", GeneratorBsonObjectIdModel.Create() }, { "3", GeneratorBsonObjectIdModel.Create() } }, null,
                new() { { "4", GeneratorBooleanModel.Create() }, { "5", GeneratorBooleanModel.Create() } }, new() { { "6", GeneratorByteArrayAsGenericModel.Create() }, { "7", null } }, null);
        }

        public bool Equals(GeneratorGenericModel<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> other)
        {
            return other != null &&
                   BsonType == other.BsonType &&
                   DictionaryBsonType == other.DictionaryBsonType &&
                   Property.Equals(other.Property) &&
                   NullableProperty.Equals(other.NullableProperty) &&
                   AlwaysNullProperty.Equals(other.AlwaysNullProperty) &&
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
    }
    //TODO: Генератор для генерика + словарь коряво генерит вриты 1023 строка
    public class GeneratorGenericsTest : SerializationTestBase
    {
        [Fact]
        public async Task GenericsTest()
        {
            var model = GeneratorGenericModel<string, int, double, bool, BsonDocument, BsonObjectId, BsonTimestamp, Guid, DateTimeOffset, long, GeneratorBsonObjectIdModel, GeneratorGuidModel, GeneratorBooleanModel, GeneratorByteArrayAsGenericModel, GeneratorBsonTimestampModel>.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
    }
}
