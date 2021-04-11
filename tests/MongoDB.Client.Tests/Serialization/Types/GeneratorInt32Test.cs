﻿using System;
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
    public partial class GeneratorInt32Model : GeneratorTypeTestModelBase<int, int?>, IEquatable<GeneratorInt32Model>
    {
        public GeneratorInt32Model(
            int property, 
            int? nullableProperty, 
            int? alwaysNullProperty, 
            List<int> listProperty, 
            List<int>? nullableListProperty, 
            List<int>? alwaysNullListProperty, 
            List<int?> listWithNullableTypeArgumentProperty,
            List<int?>? nullableListWithNullableTypeArgumentProperty, 
            List<int?>? alwaysNullListWithNullableTypeArgumentProperty, 
            Dictionary<string, int> dictionaryProperty, 
            Dictionary<string, int>? nullableDictionaryProperty, 
            Dictionary<string, int>? alwaysNullDictionaryProperty, 
            Dictionary<string, int?> dictionaryWithNullableTypeArgument, 
            Dictionary<string, int?>? nullableDictionaryWithNullableTypeArgument,
            Dictionary<string, int?>? alwaysNullDictionaryWithNullableTypeArgument) 
            : base (property, nullableProperty, alwaysNullProperty, 
                    listProperty, nullableListProperty, alwaysNullListProperty, 
                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
        {
            BsonType = BsonElementType.Int32;
            DictionaryBsonType = BsonElementType.Int32;
        }
        public override bool Equals(BsonDocument doc)
        {
            return base.Equals(doc);
        }
        public static GeneratorInt32Model Create()
        {
            return new GeneratorInt32Model(
                42, 42, null, 
                new() { 42, 42}, new() { 42, 42}, null, 
                new() { 42, null}, new() { 42, null}, null,
                new() { { "42", 42 }, { "24", 24 } }, new() { { "42", 42 }, { "24", 24 } }, null, 
                new() { { "42", 42 }, { "24", 24 } }, new() { { "42", 42 }, { "24", null }  }, null);
        }

        public bool Equals(GeneratorInt32Model other)
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
    }


    public class GeneratorInt32Test : SerializationTestBase
    {
        [Fact]
        public async Task Int32Test()
        {
            var model = GeneratorInt32Model.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(GeneratorInt32Model.Create());
            Assert.Equal(model, result);
            model.Equals(bson);
        }
    }
}