using System.Text;
using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static INamedTypeSymbol System_Byte => BsonSerializerGenerator.Compilation.GetSpecialType(SpecialType.System_Byte);
        public static INamedTypeSymbol System_Int32 => BsonSerializerGenerator.Compilation.GetSpecialType(SpecialType.System_Int32);
        public static INamedTypeSymbol System_String => BsonSerializerGenerator.Compilation.GetSpecialType(SpecialType.System_String);
        public static INamedTypeSymbol System_Memory => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Memory`1")!;
        public static INamedTypeSymbol BsonReaderTypeSym => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Reader.BsonReader")!;
        public static INamedTypeSymbol BsonWriterTypeSym => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Writer.BsonWriter")!;
        public static INamedTypeSymbol System_DateTimeOffset => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.DateTimeOffset")!;
        public static INamedTypeSymbol System_Guid => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Guid")!;
        public static INamedTypeSymbol BsonTimestamp => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonTimestamp")!;
        public static INamedTypeSymbol BsonObjectId => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonObjectId")!;
        public static INamedTypeSymbol BsonArray => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonArray")!;
        public static INamedTypeSymbol BsonDocument => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonDocument")!;
        public static INamedTypeSymbol System_Collections_Generic_IList_T => BsonSerializerGenerator.Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
        public static INamedTypeSymbol System_Collections_Generic_List_T => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!;
        public static INamedTypeSymbol System_Collections_Generic_IReadOnlyCollection_T => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyCollection`1")!;
        public static INamedTypeSymbol System_Collections_Generic_IReadOnlyList_T => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyList`1")!;
        public static INamedTypeSymbol System_Collections_Generic_ICollection_T => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.ICollection`1")!;
        public static INamedTypeSymbol System_Collections_Generic_Dictionary_K_V => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.Dictionary`2")!;
        public static INamedTypeSymbol System_Collections_Generic_IDictionary_K_V => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.IDictionary`2")!;
        public static INamedTypeSymbol System_Collections_Generic_IReadOnlyDictionary_K_V => BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Collections.Generic.IReadOnlyDictionary`2")!;
        public static ITypeSymbol ArrayByteTypeSym => BsonSerializerGenerator.Compilation.CreateArrayTypeSymbol(System_Byte);
        public static ITypeSymbol MemoryByteTypeSym => System_Memory.Construct(System_Byte);
        public static bool TryGetMetadata(ITypeSymbol source, out ISymbol result)
        {
            var str = source.ToString();
            result = BsonSerializerGenerator.Compilation.GetTypeByMetadataName(str);
            if (result != null)
            {
                return true;
            }
            while (true)
            {
                var last = str.LastIndexOf('.');
                if (last == -1)
                {
                    break;
                }
                StringBuilder builder = new StringBuilder(str);
                str = builder.Replace('.', '+', last, 1).ToString();

                result = BsonSerializerGenerator.Compilation.GetTypeByMetadataName(str);
                if (result != null)
                {
                    return true;
                }

            }
            return false;
        }
        public static bool IsArrayByteOrMemoryByte(ISymbol sym)
        {
            return sym.Equals(ArrayByteTypeSym, SymbolEqualityComparer.Default) ||
                   sym.Equals(MemoryByteTypeSym, SymbolEqualityComparer.Default);
        }
        public static bool IsBsonTimestamp(ISymbol sym)
        {
            return sym.Equals(BsonTimestamp, SymbolEqualityComparer.Default);
        }
        public static bool IsEnum(ISymbol symbol)
        {
            return symbol is ITypeSymbol namedType && namedType.TypeKind == TypeKind.Enum;
        }
        public static bool HaveCollectionIndexator(ISymbol symbol)
        {
            if (IsListCollection(symbol) == false)
            {
                return false;
            }
            return symbol.OriginalDefinition.Equals(System_Collections_Generic_List_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IList_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IReadOnlyList_T, SymbolEqualityComparer.Default);
        }
        public static bool IsListCollection(ISymbol symbol)
        {
            return symbol.OriginalDefinition.Equals(System_Collections_Generic_List_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IList_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IReadOnlyCollection_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IReadOnlyList_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_ICollection_T, SymbolEqualityComparer.Default);
        }
        public static bool IsDictionaryCollection(ISymbol symbol)
        {
            return symbol.OriginalDefinition.Equals(System_Collections_Generic_Dictionary_K_V, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IDictionary_K_V, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_IReadOnlyDictionary_K_V, SymbolEqualityComparer.Default);
        }
        public static bool IsBsonObjectId(ISymbol sym)
        {
            return sym.Equals(BsonObjectId, SymbolEqualityComparer.Default);
        }
        public static bool IsBsonDocument(ISymbol sym)
        {
            return sym.Equals(BsonDocument, SymbolEqualityComparer.Default);
        }
        public static bool IsBsonArray(ISymbol sym)
        {
            return sym.Equals(BsonArray, SymbolEqualityComparer.Default);
        }
        public static bool IsDateTimeOffset(ISymbol sym)
        {
            return sym.Equals(System_DateTimeOffset, SymbolEqualityComparer.Default);
        }
        public static bool IsGuid(ISymbol sym)
        {
            return sym.Equals(System_Guid, SymbolEqualityComparer.Default);
        }
    }
}
