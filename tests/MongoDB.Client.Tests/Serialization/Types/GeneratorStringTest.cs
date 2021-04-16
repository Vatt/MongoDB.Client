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
    public partial class GeneratorStringModel : GeneratorTypeTestModelBase<string, string?>, IEquatable<GeneratorStringModel>
    {
        public GeneratorStringModel(
            string property,
            string? nullableProperty,
            string? alwaysNullProperty, 
            List<string> listProperty, 
            List<string>? nullableListProperty, 
            List<string>? alwaysNullListProperty, 
            List<string?> listWithNullableTypeArgumentProperty,
            List<string?>? nullableListWithNullableTypeArgumentProperty, 
            List<string?>? alwaysNullListWithNullableTypeArgumentProperty, 
            Dictionary<string, string> dictionaryProperty, 
            Dictionary<string, string>? nullableDictionaryProperty, 
            Dictionary<string, string>? alwaysNullDictionaryProperty, 
            Dictionary<string, string?> dictionaryWithNullableTypeArgument, 
            Dictionary<string, string?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, string?>? alwaysNullDictionaryWithNullableTypeArgument) 
            : base (property, nullableProperty, alwaysNullProperty, 
                    listProperty, nullableListProperty, alwaysNullListProperty, 
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.String;
            DictionaryBsonType = BsonElementType.String;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static GeneratorStringModel Create()
        {
            return new GeneratorStringModel(
                "42", "42", null, 
                new() { "42", "42" }, new() { "42", "42" }, null, 
                new() { "42", null}, new() { "42", null}, null,
                new() { { "42", "42" }, { "24", "24" } }, new() { { "42", "42" }, { "24", "24" } }, null, 
                new() { { "42", "42" }, { "24", "24" } }, new() { { "42", "42" }, { "24", null }  }, null);
        }

        public bool Equals(GeneratorStringModel other)
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
            return Equals(obj as GeneratorStringModel);
        }
    }


    public class GeneratorStringTest : SerializationTestBase
    {
        [Fact]
        public async Task StringTest()
        {
            var model = GeneratorStringModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
