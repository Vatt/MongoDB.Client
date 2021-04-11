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
    public partial class GeneratorBsonDocumentModel : GeneratorTypeTestModelBase<BsonDocument, BsonDocument?>, IEquatable<GeneratorBsonDocumentModel>
    {
        public GeneratorBsonDocumentModel(
            BsonDocument property, 
            BsonDocument? nullableProperty, 
            BsonDocument? alwaysNullProperty, 
            List<BsonDocument> listProperty, 
            List<BsonDocument>? nullableListProperty, 
            List<BsonDocument>? alwaysNullListProperty, 
            List<BsonDocument?> listWithNullableTypeArgumentProperty,
            List<BsonDocument?>? nullableListWithNullableTypeArgumentProperty, 
            List<BsonDocument?>? alwaysNullListWithNullableTypeArgumentProperty, 
            Dictionary<string, BsonDocument> dictionaryProperty, 
            Dictionary<string, BsonDocument>? nullableDictionaryProperty, 
            Dictionary<string, BsonDocument>? alwaysNullDictionaryProperty, 
            Dictionary<string, BsonDocument?> dictionaryWithNullableTypeArgument, 
            Dictionary<string, BsonDocument?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, BsonDocument?>? alwaysNullDictionaryWithNullableTypeArgument) 
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
        public static GeneratorBsonDocumentModel Create()
        {
            var value = new BsonDocument("BsonDocument", BsonObjectId.NewObjectId());
            return new GeneratorBsonDocumentModel(
                value, value, null, 
                new() { value, value }, new() { value, value }, null, 
                new() { value, null}, new() { value, null}, null,
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null, 
                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null }  }, null);
        }

        public bool Equals(GeneratorBsonDocumentModel other)
        {
            return other != null &&
                   BsonType.Equals(other.BsonType) &&
                   DictionaryBsonType.Equals(other.DictionaryBsonType) &&
                   Property.Equals(other.Property) &&
                   NullableProperty.Equals(other.NullableProperty) &&
                   AlwaysNullProperty is null && other.AlwaysNullProperty is null &&
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
    }


    public class GeneratorBsonDocumentTest : SerializationTestBase
    {
        [Fact]
        public async Task BsonDocumentTest()
        {
            var model = GeneratorBsonDocumentModel.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(GeneratorBsonDocumentModel.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}
