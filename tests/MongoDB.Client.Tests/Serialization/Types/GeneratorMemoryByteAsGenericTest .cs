using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    class ListOfNullableMemoryByteEqualityComparer : IEqualityComparer<Memory<byte>?>
    {
        public bool Equals(Memory<byte>? x, Memory<byte>? y)
        {
            if (x.HasValue && y.HasValue && x.Value.Length != y.Value.Length)
            {
                return false;
            }
            if (x.HasValue == false && y.HasValue == false)
            {
                return true;
            }
            return x.Value.Span.SequenceEqual(y.Value.Span);
        }

        public int GetHashCode([DisallowNull] Memory<byte>? obj)
        {
            var hash = new HashCode();
            hash.Add(obj);
            return hash.ToHashCode();
        }
    }
    class DictionaryOfNullableMemoryByteEqualityComparer : IEqualityComparer<KeyValuePair<string, Memory<byte>?>>
    {
        public bool Equals(KeyValuePair<string, Memory<byte>?> x, KeyValuePair<string, Memory<byte>?> y)
        {
            if (x.Key.Equals(y.Key) == false)
            {
                return false;
            }
            if (x.Value.HasValue && y.Value.HasValue && x.Value.Value.Span.Length != y.Value.Value.Span.Length)
            {
                return false;
            }
            if (x.Value.HasValue == false && y.Value.HasValue == false)
            {
                return true;
            }
            return x.Value.Value.Span.SequenceEqual(y.Value.Value.Span);
        }

        public int GetHashCode([DisallowNull] KeyValuePair<string, Memory<byte>?> obj)
        {
            var hash = new HashCode();
            hash.Add(obj.Key);
            hash.Add(obj.Value);
            return hash.ToHashCode();
        }
    }
    class ListOfMemoryByteEqualityComparer : IEqualityComparer<Memory<byte>>
    {
        public bool Equals(Memory<byte> x, Memory<byte> y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }

            return x.Span.SequenceEqual(y.Span);
        }

        public int GetHashCode([DisallowNull] Memory<byte> obj)
        {
            var hash = new HashCode();
            hash.Add(obj);
            return hash.ToHashCode();
        }
    }
    class DictionaryOfMemoryByteEqualityComparer : IEqualityComparer<KeyValuePair<string, Memory<byte>>>
    {
        public bool Equals(KeyValuePair<string, Memory<byte>> x, KeyValuePair<string, Memory<byte>> y)
        {
            if (x.Key.Equals(y.Key) == false)
            {
                return false;
            }
            if (x.Value.Length != y.Value.Length)
            {
                return false;
            }
            return x.Value.Span.SequenceEqual(y.Value.Span);
        }

        public int GetHashCode([DisallowNull] KeyValuePair<string, Memory<byte>> obj)
        {
            var hash = new HashCode();
            hash.Add(obj.Key);
            hash.Add(obj.Value);
            return hash.ToHashCode();
        }
    }
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class GeneratorMemoryByteAsGenericModel : GeneratorTypeTestModelBase<Memory<byte>, Memory<byte>?>, IEquatable<GeneratorMemoryByteAsGenericModel>
    {
        public GeneratorMemoryByteAsGenericModel(
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
            : base (property, nullableProperty, alwaysNullProperty, 
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
        public static GeneratorMemoryByteAsGenericModel Create()
        {
            var value = new byte[]{ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            return new GeneratorMemoryByteAsGenericModel(
                value, value, null, 
                new() { value, value }, new() { value, value }, null, 
                new() { value, null}, new() { value, null}, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null, 
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null }  }, null);
        }

        public bool Equals(GeneratorMemoryByteAsGenericModel other)
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
            return Equals(obj as GeneratorMemoryByteAsGenericModel);
        }
    }


    public class GeneratorMemoryByteAsGeneric : SerializationTestBase
    {
        [Fact]
        public async Task MemoryByteAsGenericTest()
        {
            var model = GeneratorMemoryByteAsGenericModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(GeneratorMemoryByteAsGenericModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
