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
    public partial class GeneratorBsonObjectIdModel : GeneratorTypeTestModelBase<BsonObjectId, BsonObjectId?>, IEquatable<GeneratorBsonObjectIdModel>
    {
        public GeneratorBsonObjectIdModel(
            BsonObjectId property, 
            BsonObjectId? nullableProperty, 
            BsonObjectId? alwaysNullProperty, 
            List<BsonObjectId> listProperty, 
            List<BsonObjectId>? nullableListProperty, 
            List<BsonObjectId>? alwaysNullListProperty, 
            List<BsonObjectId?> listWithNullableTypeArgumentProperty,
            List<BsonObjectId?>? nullableListWithNullableTypeArgumentProperty, 
            List<BsonObjectId?>? alwaysNullListWithNullableTypeArgumentProperty, 
            Dictionary<string, BsonObjectId> dictionaryProperty, 
            Dictionary<string, BsonObjectId>? nullableDictionaryProperty, 
            Dictionary<string, BsonObjectId>? alwaysNullDictionaryProperty, 
            Dictionary<string, BsonObjectId?> dictionaryWithNullableTypeArgument, 
            Dictionary<string, BsonObjectId?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, BsonObjectId?>? alwaysNullDictionaryWithNullableTypeArgument) 
            : base (property, nullableProperty, alwaysNullProperty, 
                    listProperty, nullableListProperty, alwaysNullListProperty, 
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.UtcDateTime;
            DictionaryBsonType = BsonElementType.UtcDateTime;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static GeneratorBsonObjectIdModel Create()
        {
            var value = BsonObjectId.NewObjectId();
            return new GeneratorBsonObjectIdModel(
                value, value, null, 
                new() { value, value }, new() { value, value }, null, 
                new() { value, null}, new() { value, null}, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null, 
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null }  }, null);
        }

        public bool Equals(GeneratorBsonObjectIdModel other)
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
            return Equals(obj as GeneratorBsonObjectIdModel);
        }
    }


    public class GeneratorBsonObjectIdTest : SerializationTestBase
    {
        [Fact]
        public async Task BsonObjectIdTest()
        {
            var model = GeneratorBsonObjectIdModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(GeneratorBsonObjectIdModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
