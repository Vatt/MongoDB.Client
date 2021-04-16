﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Client.Bson.Reader;
using MongoDB.Client.Bson.Serialization.Attributes;
using MongoDB.Client.Bson.Writer;
using Xunit;

namespace MongoDB.Client.Tests.Serialization.Generator
{
    public readonly struct StructForExtension : IEquatable<StructForExtension>
    {
        public readonly int A;
        public readonly int B;
        public readonly int C;
        public StructForExtension(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public override bool Equals(object obj)
        {
            return obj is StructForExtension extension && Equals(extension);
        }

        public bool Equals(StructForExtension other)
        {
            return A == other.A &&
                   B == other.B &&
                   C == other.C;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }
    }
    public static class StructForExtensionSerializer
    {
        public static bool TryParseBson(ref BsonReader reader, out StructForExtension message)
        {
            message = default;
            if(reader.TryGetString(out var value) == false)
            {
                return false;
            }
            var splitted = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
            message = new StructForExtension(int.Parse(splitted[0]), int.Parse(splitted[1]), int.Parse(splitted[2]));
            return true;
        }
        public static void WriteBson(ref BsonWriter writer, in StructForExtension message, out byte bsonType)
        {
            bsonType = 2;
            writer.WriteString($"{message.A};{message.B};{message.C}");
        }
    }

    [BsonSerializable]
    public partial class GeneratorModelWithExtension : IEquatable<GeneratorModelWithExtension>
    {
        public string StringProp { get; }
        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public StructForExtension ExtensionProp { get; }
        public GeneratorModelWithExtension(string stringProp, StructForExtension extensionProp)
        {
            StringProp = stringProp;
            ExtensionProp = extensionProp;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GeneratorModelWithExtension);
        }

        public bool Equals(GeneratorModelWithExtension other)
        {
            return other != null &&
                   StringProp == other.StringProp &&
                   ExtensionProp.Equals(other.ExtensionProp);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StringProp, ExtensionProp);
        }
    }
    public class CustomModel : IEquatable<CustomModel>
    {
        public int Prop0 { get; }
        public int Prop1 { get; }
        public int Prop2 { get; }
        public CustomModel(int prop0, int prop1, int prop2)
        {
            Prop0 = prop0;
            Prop1 = prop1;
            Prop2 = prop2;
        }
        private static ReadOnlySpan<byte> CustomModelProp0 => new byte[5] { 80, 114, 111, 112, 48 };
        private static ReadOnlySpan<byte> CustomModelProp1 => new byte[5] { 80, 114, 111, 112, 49 };
        private static ReadOnlySpan<byte> CustomModelProp2 => new byte[5] { 80, 114, 111, 112, 50 };
        public static bool TryParseBson(ref MongoDB.Client.Bson.Reader.BsonReader reader, out MongoDB.Client.Tests.Serialization.Generator.CustomModel message)
        {
            message = default;
            int Int32Prop0 = default;
            int Int32Prop1 = default;
            int Int32Prop2 = default;
            if (!reader.TryGetInt32(out int docLength))
            {
                return false;
            }

            var unreaded = reader.Remaining + sizeof(int);
            while (unreaded - reader.Remaining < docLength - 1)
            {
                if (!reader.TryGetByte(out var bsonType))
                {
                    return false;
                }

                if (!reader.TryGetCStringAsSpan(out var bsonName))
                {
                    return false;
                }

                if (bsonType == 10)
                {
                    continue;
                }

                if (bsonName.Length < 4)
                {
                    if (!reader.TrySkip(bsonType))
                    {
                        return false;
                    }

                    continue;
                }

                switch (System.Runtime.CompilerServices.Unsafe.Add(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(bsonName), (nint)4))
                {
                    case 48:
                        {
                            if (bsonName.SequenceEqual5(CustomModelProp0))
                            {
                                if (!reader.TryGetInt32(out Int32Prop0))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 49:
                        {
                            if (bsonName.SequenceEqual5(CustomModelProp1))
                            {
                                if (!reader.TryGetInt32(out Int32Prop1))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }

                    case 50:
                        {
                            if (bsonName.SequenceEqual5(CustomModelProp2))
                            {
                                if (!reader.TryGetInt32(out Int32Prop2))
                                {
                                    return false;
                                }

                                continue;
                            }

                            break;
                        }
                }

                if (!reader.TrySkip(bsonType))
                {
                    return false;
                }
            }

            if (!reader.TryGetByte(out var endMarker))
            {
                return false;
            }

            if (endMarker != 0)
            {
                throw new MongoDB.Client.Bson.Serialization.Exceptions.SerializerEndMarkerException(nameof(MongoDB.Client.Tests.Serialization.Generator.CustomModel), endMarker);
            }

            message = new MongoDB.Client.Tests.Serialization.Generator.CustomModel(prop0: Int32Prop0, prop1: Int32Prop1, prop2: Int32Prop2);
            return true;
        }

        public static void WriteBson(ref MongoDB.Client.Bson.Writer.BsonWriter writer, in MongoDB.Client.Tests.Serialization.Generator.CustomModel message)
        {
            var checkpoint = writer.Written;
            var reserved = writer.Reserve(4);
            writer.Write_Type_Name_Value(CustomModelProp0, message.Prop0);
            writer.Write_Type_Name_Value(CustomModelProp1, message.Prop1);
            writer.Write_Type_Name_Value(CustomModelProp2, message.Prop2);
            writer.WriteByte(0);
            var docLength = writer.Written - checkpoint;
            reserved.Write(docLength);
            writer.Commit();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CustomModel);
        }

        public bool Equals(CustomModel other)
        {
            return other != null &&
                   Prop0 == other.Prop0 &&
                   Prop1 == other.Prop1 &&
                   Prop2 == other.Prop2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Prop0, Prop1, Prop2);
        }
    }
    [BsonSerializable]
    public partial class GeneratorModelWithExtensionAndCustom
    {
        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public StructForExtension Property { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public StructForExtension? NullableProperty { get; }

        public CustomModel? AlwaysNullProperty { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public List<StructForExtension> ListProperty { get; }

        public List<CustomModel>? NullableListProperty { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public List<StructForExtension>? AlwaysNullListProperty { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public List<StructForExtension?> ListWithNullableTypeArgumentProperty { get; }

        public List<CustomModel?>? NullableListWithNullableTypeArgumentProperty { get; }

        public List<CustomModel?>? AlwaysNullListWithNullableTypeArgumentProperty { get; }
        
        public Dictionary<string, CustomModel> DictionaryProperty { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public Dictionary<string, StructForExtension>? NullableDictionaryProperty { get; }
        
        public Dictionary<string, CustomModel>? AlwaysNullDictionaryProperty { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public Dictionary<string, StructForExtension?> DictionaryWithNullableTypeArgument { get; }

        public Dictionary<string, CustomModel>? NullableDictionaryWithNullableTypeArgument { get; }

        [BsonSerializer(typeof(StructForExtensionSerializer))]
        public Dictionary<string, StructForExtension?>? AlwaysNullDictionaryWithNullableTypeArgument { get; }

        public GeneratorModelWithExtensionAndCustom(
             StructForExtension property,
             StructForExtension? nullableProperty,
             CustomModel? alwaysNullProperty,
             List<StructForExtension> listProperty,
             List<CustomModel>? nullableListProperty,
             List<StructForExtension>? alwaysNullListProperty,
             List<StructForExtension?> listWithNullableTypeArgumentProperty,
             List<CustomModel?>? nullableListWithNullableTypeArgumentProperty,
             List<CustomModel?>? alwaysNullListWithNullableTypeArgumentProperty,
             Dictionary<string, CustomModel> dictionaryProperty,
             Dictionary<string, StructForExtension>? nullableDictionaryProperty,
             Dictionary<string, CustomModel>? alwaysNullDictionaryProperty,
             Dictionary<string, StructForExtension?> dictionaryWithNullableTypeArgument,
             Dictionary<string, CustomModel?>? nullableDictionaryWithNullableTypeArgument,
             Dictionary<string, StructForExtension?>? alwaysNullDictionaryWithNullableTypeArgument)
        {
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
        public static GeneratorModelWithExtensionAndCustom Create()
        {
            var extension = new StructForExtension(1, 2, 3);
            var custom = new CustomModel(1, 2, 3);
            return new GeneratorModelWithExtensionAndCustom(
                extension, extension, null,
                new() { extension, extension }, new() { custom, custom }, null,
                new() { extension, null }, new() { custom, null }, null,
                new() { { "42", custom }, { "24", custom } }, new() { { "42", extension }, { "24", extension } }, null,
                new() { { "42", extension }, { "24", extension } }, new() { { "42", custom }, { "24", null } }, null);
        }

        public bool Equals(GeneratorModelWithExtensionAndCustom other)
        { 
            return other != null &&
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
            return Equals(obj as GeneratorModelWithExtensionAndCustom);
        }
    }
    public class GeneratorCustomsTest : SerializationTestBase
    {
        [Fact]
        public async Task ExtensionTest()
        {
            var model = new GeneratorModelWithExtension("Extension", new StructForExtension(1, 2, 3));
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task CustomTest()
        {
            var model = new CustomModel(1, 2, 3);
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
        [Fact]
        public async Task CustomAndExtensionTest()
        {
            var model = GeneratorModelWithExtensionAndCustom.Create();
            var result = await RoundTripAsync(model);
            var bson = await RoundTripWithBsonAsync(model);
            Assert.Equal(model, result);
        }
    }
}
