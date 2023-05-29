using System.Security.Cryptography;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{

    [BsonSerializable(GeneratorMode.ConstructorOnlyParameters)]
    public partial class GeneratorMemoryByteAsMD5Model : IEquatable<GeneratorMemoryByteAsMD5Model>
    {
        protected BsonElementType BsonType;
        protected BsonElementType DictionaryBsonType;

        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Memory<byte> Property { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Memory<byte>? NullableProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Memory<byte>? AlwaysNullProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<Memory<byte>> ListProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<Memory<byte>>? NullableListProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<Memory<byte>>? AlwaysNullListProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<Memory<byte>?> ListWithNullableTypeArgumentProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<Memory<byte>?>? NullableListWithNullableTypeArgumentProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<Memory<byte>?>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, Memory<byte>> DictionaryProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, Memory<byte>>? NullableDictionaryProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, Memory<byte>>? AlwaysNullDictionaryProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, Memory<byte>?> DictionaryWithNullableTypeArgument { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, Memory<byte>?>? NullableDictionaryWithNullableTypeArgument { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, Memory<byte>?>? AlwaysNullDictionaryWithNullableTypeArgument { get; }

        //TODO: Fix it, уточнение к параметрам констурктора при режиме только конструктор приоритетно
        public GeneratorMemoryByteAsMD5Model(
             Memory<byte> property,
             Memory<byte>? nullableProperty,
             Memory<byte>? alwaysNullProperty,
             List<Memory<byte>> listProperty,
             List<Memory<byte>>? nullableListProperty,
             List<Memory<byte>>? alwaysNullListProperty,
             List<Memory<byte>?> listWithNullableTypeArgumentProperty,
             List<Memory<byte>?>? nullableListWithNullableTypeArgumentProperty,
             List<Memory<byte>?>? alwaysNullListWithNullableTypeArgumentProperty,
             Dictionary<string, Memory<byte>> dictionaryProperty,
             Dictionary<string, Memory<byte>>? nullableDictionaryProperty,
             Dictionary<string, Memory<byte>>? alwaysNullDictionaryProperty,
             Dictionary<string, Memory<byte>?> dictionaryWithNullableTypeArgument,
             Dictionary<string, Memory<byte>?>? nullableDictionaryWithNullableTypeArgument,
             Dictionary<string, Memory<byte>?>? alwaysNullDictionaryWithNullableTypeArgument)
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
        public static MemoryByteAsGenericModel Create()
        {
            var value = MD5.HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
            return new MemoryByteAsGenericModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(GeneratorMemoryByteAsMD5Model other)
        {
            var listComparer = new ListOfMemoryByteEqualityComparer();
            var dictionaryComparer = new DictionaryOfMemoryByteEqualityComparer();
            var nullableListComparer = new ListOfNullableMemoryByteEqualityComparer();
            var nullableDictionaryComparer = new DictionaryOfNullableMemoryByteEqualityComparer();
            return other != null &&
                   BsonType == other.BsonType &&
                   DictionaryBsonType == other.DictionaryBsonType &&
                   Property.Span.SequenceEqual(other.Property.Span) &&
                   NullableProperty.Value.Span.SequenceEqual(other.NullableProperty.Value.Span) &&
                   AlwaysNullProperty is null && other.AlwaysNullProperty is null &&
                   ListProperty.SequenceEqual(other.ListProperty, listComparer) &&
                   NullableListProperty.SequenceEqual(other.NullableListProperty, listComparer) &&
                   AlwaysNullListProperty is null && other.AlwaysNullListProperty is null &&
                   ListWithNullableTypeArgumentProperty.SequenceEqual(other.ListWithNullableTypeArgumentProperty, nullableListComparer) &&
                   NullableListWithNullableTypeArgumentProperty.SequenceEqual(other.NullableListWithNullableTypeArgumentProperty, nullableListComparer) &&
                   AlwaysNullListWithNullableTypeArgumentProperty is null && other.AlwaysNullListWithNullableTypeArgumentProperty is null &&
                   DictionaryProperty.SequenceEqual(other.DictionaryProperty, dictionaryComparer) &&
                   NullableDictionaryProperty.SequenceEqual(other.NullableDictionaryProperty, dictionaryComparer) &&
                   AlwaysNullDictionaryProperty is null && other.AlwaysNullDictionaryProperty is null &&
                   DictionaryWithNullableTypeArgument.SequenceEqual(other.DictionaryWithNullableTypeArgument, nullableDictionaryComparer) &&
                   NullableDictionaryWithNullableTypeArgument.SequenceEqual(other.NullableDictionaryWithNullableTypeArgument, nullableDictionaryComparer) &&
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
            return Equals(obj as GeneratorMemoryByteAsMD5Model);
        }
    }


    public class GeneratorMemoryByteAsMD5Test : SerializationTestBase
    {
        [Fact]
        public async Task MemoryByteAsMD5Test()
        {
            var model = GeneratorMemoryByteAsMD5Model.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(MemoryByteAsGenericModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
