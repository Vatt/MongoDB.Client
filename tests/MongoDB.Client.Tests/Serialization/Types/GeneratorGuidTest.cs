using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Document;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Tests.Serialization.Generator;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Types
{
    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
    public partial class GeneratorGuidModel : GeneratorTypeTestModelBase<Guid, Guid?>, IEquatable<GeneratorGuidModel>
    {
        public GeneratorGuidModel(
            Guid property, 
            Guid? nullableProperty, 
            Guid? alwaysNullProperty, 
            List<Guid> listProperty, 
            List<Guid>? nullableListProperty, 
            List<Guid>? alwaysNullListProperty, 
            List<Guid?> listWithNullableTypeArgumentProperty,
            List<Guid?>? nullableListWithNullableTypeArgumentProperty, 
            List<Guid?>? alwaysNullListWithNullableTypeArgumentProperty, 
            Dictionary<string, Guid> dictionaryProperty, 
            Dictionary<string, Guid>? nullableDictionaryProperty, 
            Dictionary<string, Guid>? alwaysNullDictionaryProperty, 
            Dictionary<string, Guid?> dictionaryWithNullableTypeArgument, 
            Dictionary<string, Guid?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, Guid?>? alwaysNullDictionaryWithNullableTypeArgument) 
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
        public static GeneratorGuidModel Create()
        {
            var value = Guid.NewGuid();
            return new GeneratorGuidModel(
                value, value, null, 
                new() { value, value }, new() { value, value }, null, 
                new() { value, null}, new() { value, null}, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null, 
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null }  }, null);
        }

        public bool Equals(GeneratorGuidModel other)
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
            return Equals(obj as GeneratorGuidModel);
        }
    }


    public class GeneratorGuidTest : SerializationTestBase
    {
        [Fact]
        public async Task GuidTest()
        {
            var model = GeneratorGuidModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(GeneratorGuidModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
