using System.Security.Cryptography;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{

    [BsonSerializable(GeneratorMode.ConstructorParameters)]
    public partial class GeneratorByteArrayAsMD5Model : IEquatable<GeneratorByteArrayAsMD5Model>
    {
        protected BsonType BsonType;
        protected BsonType DictionaryBsonType;

        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public byte[] Property { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public byte[]? NullableProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public byte[]? AlwaysNullProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<byte[]> ListProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<byte[]>? NullableListProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<byte[]>? AlwaysNullListProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<byte[]?> ListWithNullableTypeArgumentProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<byte[]?>? NullableListWithNullableTypeArgumentProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public List<byte[]?>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, byte[]> DictionaryProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, byte[]>? NullableDictionaryProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, byte[]>? AlwaysNullDictionaryProperty { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, byte[]?> DictionaryWithNullableTypeArgument { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, byte[]?>? NullableDictionaryWithNullableTypeArgument { get; }
        [BsonBinaryData(BinaryDataRepresentation.MD5)]
        public Dictionary<string, byte[]?>? AlwaysNullDictionaryWithNullableTypeArgument { get; }

        //TODO: Fix it, уточнение к параметрам констурктора при режиме только конструктор приоритетно
        public GeneratorByteArrayAsMD5Model(
            byte[] property,
             byte[]? nullableProperty,
             byte[]? alwaysNullProperty,
             List<byte[]> listProperty,
             List<byte[]>? nullableListProperty,
             List<byte[]>? alwaysNullListProperty,
             List<byte[]?> listWithNullableTypeArgumentProperty,
             List<byte[]?>? nullableListWithNullableTypeArgumentProperty,
             List<byte[]?>? alwaysNullListWithNullableTypeArgumentProperty,
             Dictionary<string, byte[]> dictionaryProperty,
             Dictionary<string, byte[]>? nullableDictionaryProperty,
             Dictionary<string, byte[]>? alwaysNullDictionaryProperty,
             Dictionary<string, byte[]?> dictionaryWithNullableTypeArgument,
             Dictionary<string, byte[]?>? nullableDictionaryWithNullableTypeArgument,
             Dictionary<string, byte[]?>? alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonType.BinaryData;
            DictionaryBsonType = BsonType.BinaryData;
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
        public static ByteArrayAsGenericModel Create()
        {
            var value = MD5.HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
            return new ByteArrayAsGenericModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(GeneratorByteArrayAsMD5Model other)
        {
            var listComparer = new ListOfByteArrayEqualityComparer();
            var dictionaryComparer = new DictionaryOfByteArrayEqualityComparer();
            return other != null &&
                   BsonType == other.BsonType &&
                   DictionaryBsonType == other.DictionaryBsonType &&
                   Property.SequenceEqual(other.Property, EqualityComparer<byte>.Default) &&
                   NullableProperty.SequenceEqual(other.NullableProperty, EqualityComparer<byte>.Default) &&
                   AlwaysNullProperty is null && other.AlwaysNullProperty is null &&
                   ListProperty.SequenceEqual(other.ListProperty, listComparer) &&
                   NullableListProperty.SequenceEqual(other.NullableListProperty, listComparer) &&
                   AlwaysNullListProperty is null && other.AlwaysNullListProperty is null &&
                   ListWithNullableTypeArgumentProperty.SequenceEqual(other.ListWithNullableTypeArgumentProperty, listComparer) &&
                   NullableListWithNullableTypeArgumentProperty.SequenceEqual(other.NullableListWithNullableTypeArgumentProperty, listComparer) &&
                   AlwaysNullListWithNullableTypeArgumentProperty is null && other.AlwaysNullListWithNullableTypeArgumentProperty is null &&
                   DictionaryProperty.SequenceEqual(other.DictionaryProperty, dictionaryComparer) &&
                   NullableDictionaryProperty.SequenceEqual(other.NullableDictionaryProperty, dictionaryComparer) &&
                   AlwaysNullDictionaryProperty is null && other.AlwaysNullDictionaryProperty is null &&
                   DictionaryWithNullableTypeArgument.SequenceEqual(other.DictionaryWithNullableTypeArgument, dictionaryComparer) &&
                   NullableDictionaryWithNullableTypeArgument.SequenceEqual(other.NullableDictionaryWithNullableTypeArgument, dictionaryComparer) &&
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
            return Equals(obj as GeneratorByteArrayAsMD5Model);
        }
    }


    public class GeneratorByteArrayAsMD5Test : SerializationTestBase
    {
        [Fact]
        public async Task ByteArrayAsMD5Test()
        {
            var model = GeneratorByteArrayAsMD5Model.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(ByteArrayAsGenericModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
