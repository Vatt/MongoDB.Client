//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using MongoDB.Client.Bson.Document;
//using MongoDB.Client.Bson.Serialization.Attributes;
//using MongoDB.Client.Tests.Serialization.Generator;
//using Xunit;

//namespace MongoDB.Client.Tests.Serialization.Types
//{
//    [BsonSerializable(GeneratorMode.ConstuctorOnlyParameters)]
//    public partial class BsonBinaryDataModel : GeneratorTypeTestModelBase<BsonBinaryData, BsonBinaryData?>, IEquatable<BsonBinaryDataModel>
//    {
//        public BsonBinaryDataModel(
//            BsonBinaryData property,
//            BsonBinaryData? nullableProperty,
//            BsonBinaryData? alwaysNullProperty,
//            List<BsonBinaryData> listProperty,
//            List<BsonBinaryData>? nullableListProperty,
//            List<BsonBinaryData>? alwaysNullListProperty,
//            List<BsonBinaryData?> listWithNullableTypeArgumentProperty,
//            List<BsonBinaryData?>? nullableListWithNullableTypeArgumentProperty,
//            List<BsonBinaryData?>? alwaysNullListWithNullableTypeArgumentProperty,
//            Dictionary<string, BsonBinaryData> dictionaryProperty,
//            Dictionary<string, BsonBinaryData>? nullableDictionaryProperty,
//            Dictionary<string, BsonBinaryData>? alwaysNullDictionaryProperty,
//            Dictionary<string, BsonBinaryData?> dictionaryWithNullableTypeArgument,
//            Dictionary<string, BsonBinaryData?>? nullableDictionaryWithNullableTypeArgument,
//            Dictionary<string, BsonBinaryData?>? alwaysNullDictionaryWithNullableTypeArgument)
//            : base(property, nullableProperty, alwaysNullProperty,
//                    listProperty, nullableListProperty, alwaysNullListProperty,
//                    listWithNullableTypeArgumentProperty, nullableListWithNullableTypeArgumentProperty, alwaysNullListWithNullableTypeArgumentProperty,
//                    dictionaryProperty, nullableDictionaryProperty, alwaysNullDictionaryProperty,
//                    dictionaryWithNullableTypeArgument, nullableDictionaryWithNullableTypeArgument, alwaysNullDictionaryWithNullableTypeArgument)
//        {
//            BsonType = BsonElementType.BinaryData;
//            DictionaryBsonType = BsonElementType.BinaryData;
//        }
//        public override bool Equals(BsonDocument doc)
//        {
//            return base.Equals(doc);
//        }
//        public static BsonBinaryDataModel Create()
//        {
//            var value = BsonBinaryData.Create(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 8, 9 });
//            var value1 = BsonBinaryData.Create(BsonBinaryDataType.MD5, MD5.HashData(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }));
//            return new BsonBinaryDataModel(
//                value, value, null,
//                new() { value, value }, new() { value, value }, null,
//                new() { value, null }, new() { value, null }, null,
//                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", value } }, null,
//                new() { { "42", value }, { "24", value } }, new() { { "42", value }, { "24", null } }, null);
//        }

//        public bool Equals(BsonBinaryDataModel other)
//        {
//            return other != null &&
//                   BsonType == other.BsonType &&
//                   DictionaryBsonType == other.DictionaryBsonType &&
//                   Property.Equals(other.Property) &&
//                   NullableProperty.Equals(other.NullableProperty) &&
//                   AlwaysNullProperty is null && other.AlwaysNullProperty is null &&
//                   ListProperty.SequenceEqual(other.ListProperty) &&
//                   NullableListProperty.SequenceEqual(other.NullableListProperty) &&
//                   AlwaysNullListProperty is null && other.AlwaysNullListProperty is null &&
//                   ListWithNullableTypeArgumentProperty.SequenceEqual(other.ListWithNullableTypeArgumentProperty) &&
//                   NullableListWithNullableTypeArgumentProperty.SequenceEqual(other.NullableListWithNullableTypeArgumentProperty) &&
//                   AlwaysNullListWithNullableTypeArgumentProperty is null && other.AlwaysNullListWithNullableTypeArgumentProperty is null &&
//                   DictionaryProperty.SequenceEqual(other.DictionaryProperty) &&
//                   NullableDictionaryProperty.SequenceEqual(other.NullableDictionaryProperty) &&
//                   AlwaysNullDictionaryProperty is null && other.AlwaysNullDictionaryProperty is null &&
//                   DictionaryWithNullableTypeArgument.SequenceEqual(other.DictionaryWithNullableTypeArgument) &&
//                   NullableDictionaryWithNullableTypeArgument.SequenceEqual(other.NullableDictionaryWithNullableTypeArgument) &&
//                   AlwaysNullDictionaryWithNullableTypeArgument is null && other.AlwaysNullDictionaryWithNullableTypeArgument is null;
//        }

//        public override int GetHashCode()
//        {
//            var hash = new HashCode();
//            hash.Add(BsonType);
//            hash.Add(DictionaryBsonType);
//            hash.Add(Property);
//            hash.Add(NullableProperty);
//            hash.Add(AlwaysNullProperty);
//            hash.Add(ListProperty);
//            hash.Add(NullableListProperty);
//            hash.Add(AlwaysNullListProperty);
//            hash.Add(ListWithNullableTypeArgumentProperty);
//            hash.Add(NullableListWithNullableTypeArgumentProperty);
//            hash.Add(AlwaysNullListWithNullableTypeArgumentProperty);
//            hash.Add(DictionaryProperty);
//            hash.Add(NullableDictionaryProperty);
//            hash.Add(AlwaysNullDictionaryProperty);
//            hash.Add(DictionaryWithNullableTypeArgument);
//            hash.Add(NullableDictionaryWithNullableTypeArgument);
//            hash.Add(AlwaysNullDictionaryWithNullableTypeArgument);
//            return hash.ToHashCode();
//        }

//        public override bool Equals(object obj)
//        {
//            return Equals(obj as BsonBinaryDataModel);
//        }
//    }


//    public class GeneratorBsonBinaryDataTest : SerializationTestBase
//    {
//        [Fact]
//        public async Task BsonBinaryDataTest()
//        {
//            var model = BsonBinaryDataModel.Create();
//            var result = await RoundTripAsync(model);
//            var bson = await RoundTripWithBsonAsync(BsonBinaryDataModel.Create());
//            Assert.Equal(model, result);
//            model.Equals(bson);
//        }
//    }
//}
