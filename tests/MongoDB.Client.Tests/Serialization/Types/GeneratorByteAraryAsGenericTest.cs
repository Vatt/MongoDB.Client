using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    class ListOfByteArrayEqualityComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] x, byte[] y)
        {
            if (x is not null && y is not null && x.Length != y.Length)
            {
                return false;
            }
            if (x is null && y is null)
            {
                return true;
            }
            return x.SequenceEqual(y);
        }

        public int GetHashCode([DisallowNull] byte[] obj)
        {
            var hash = new HashCode();
            hash.Add(obj);
            return hash.ToHashCode();
        }
    }
    class DictionaryOfByteArrayEqualityComparer : IEqualityComparer<KeyValuePair<string, byte[]>>
    {
        public bool Equals(KeyValuePair<string, byte[]> x, KeyValuePair<string, byte[]> y)
        {
            if (x.Key.Equals(y.Key) == false)
            {
                return false;
            }
            if (x.Value is not null && y.Value is not null && x.Value.Length != y.Value.Length)
            {
                return false;
            }
            if (x.Value is null && y.Value is null)
            {
                return true;
            }
            return x.Value.SequenceEqual(y.Value);
        }

        public int GetHashCode([DisallowNull] KeyValuePair<string, byte[]> obj)
        {
            var hash = new HashCode();
            hash.Add(obj.Key);
            hash.Add(obj.Value);
            return hash.ToHashCode();
        }
    }
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class ByteArrayAsGenericModel : GeneratorTypeTestModelBase<byte[], byte[]?>, IEquatable<ByteArrayAsGenericModel>
    {
        public ByteArrayAsGenericModel(
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
        public static ByteArrayAsGenericModel Create()
        {
            var value = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            return new ByteArrayAsGenericModel(
                value, value, null,
                new() { value, value }, new() { value, value }, null,
                new() { value, null }, new() { value, null }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
        }

        public bool Equals(ByteArrayAsGenericModel other)
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
            return Equals(obj as ByteArrayAsGenericModel);
        }
    }


    public class GeneratorByteArrayAsGeneric : SerializationTestBase
    {
        [Fact]
        public async Task ByteArrayAsGenericTest()
        {
            var model = ByteArrayAsGenericModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(ByteArrayAsGenericModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
