using Microsoft.CodeAnalysis;
using System.Text;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
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
        public static bool IsBsonTimestamp(ISymbol sym)
        {
            return sym.Equals(BsonTimestamp, SymbolEqualityComparer.Default);
        }
        public static bool IsEnum(ISymbol symbol)
        {
            return symbol is ITypeSymbol namedType && namedType.TypeKind == TypeKind.Enum;
        }
        public static bool IsListOrIList(ISymbol symbol)
        {
            return symbol.OriginalDefinition.Equals(System_Collections_Generic_IList_T, SymbolEqualityComparer.Default) ||
                   symbol.OriginalDefinition.Equals(System_Collections_Generic_List_T, SymbolEqualityComparer.Default);
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