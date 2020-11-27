using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    public class TypeLib
    {
        private static Compilation Compilation;
        public static bool TryGetMetadata(ITypeSymbol source, out ISymbol result)
        {
            result = default;
            var str = source.ToString();
            result = Compilation.GetTypeByMetadataName(str);
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

                result = Compilation.GetTypeByMetadataName(str);
                if (result != null)
                {
                    return true;
                }

            }         
            return false;
        }
        public static void TypeLibInit(Compilation compilation)
        {
            Compilation = compilation;
            System_DateTimeOffset = Compilation.GetTypeByMetadataName("System.DateTimeOffset")!;
            System_Guid = Compilation.GetTypeByMetadataName("System.Guid")!;
            BsonObjectId = Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonObjectId")!;
            BsonArray = Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonArray")!;
            BsonDocument = Compilation.GetTypeByMetadataName("MongoDB.Client.Bson.Document.BsonDocument")!;
            System_Collections_Generic_List_T = Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")!;
            System_Collections_Generic_IList_T = Compilation.GetSpecialType(SpecialType.System_Collections_Generic_IList_T);
            System_Object = Compilation.GetSpecialType(SpecialType.System_Object);
            System_Boolean = Compilation.GetSpecialType(SpecialType.System_Boolean);
            System_Int32 = Compilation.GetSpecialType(SpecialType.System_Int32);
            System_String = Compilation.GetSpecialType(SpecialType.System_String);
            System_Int64 = Compilation.GetSpecialType(SpecialType.System_Int64);
            System_Double = Compilation.GetSpecialType(SpecialType.System_Double);
            System_Nullable_T = Compilation.GetSpecialType(SpecialType.System_Nullable_T);
            System_DateTime = Compilation.GetSpecialType(SpecialType.System_DateTime);
            System_Decimal = Compilation.GetSpecialType(SpecialType.System_Decimal);
            System_Enum = Compilation.GetSpecialType(SpecialType.System_Enum);
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
        public static ISymbol BsonDocument;
        public static ISymbol BsonArray;
        public static ISymbol BsonObjectId;
        public static ISymbol System_Collections_Generic_List_T;
        public static ISymbol System_Collections_Generic_IList_T;
        public static ISymbol System_Object;
        public static ISymbol System_Boolean;
        public static ISymbol System_Int32;
        public static ISymbol System_String;
        public static ISymbol System_Int64;
        public static ISymbol System_Double;
        public static ISymbol System_Nullable_T;
        public static ISymbol System_DateTime;
        public static ISymbol System_Decimal;
        public static ISymbol System_Enum;
        public static ISymbol System_DateTimeOffset;       
        public static ISymbol System_Guid;       
    }
}