using System.Text;
using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static INamedTypeSymbol _systemDateTimeOffset;
        private static INamedTypeSymbol _systemGuid;
        private static INamedTypeSymbol _bsonObjectId;
        private static INamedTypeSymbol _bsonArray;
        private static INamedTypeSymbol _bsonDocument;
        public static INamedTypeSymbol System_DateTimeOffset => _systemDateTimeOffset ??= BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.DateTimeOffset")!;
        public static INamedTypeSymbol System_Guid => _systemGuid ??= BsonSerializerGenerator.Compilation.GetTypeByMetadataName("System.Guid")!;
        public static INamedTypeSymbol BsonObjectId => _bsonObjectId ??= BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonObjectId")!;
        public static INamedTypeSymbol BsonArray => _bsonArray ??= BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonArray")!;
        public static INamedTypeSymbol BsonDocument => _bsonDocument ??= BsonSerializerGenerator.Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonDocument")!;
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
        public static bool IsListOrIList(ISymbol symbol)
        {
            return symbol.ToString().Contains("System.Collections.Generic.List") || symbol.ToString().Contains("System.Collections.Generic.IList");
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