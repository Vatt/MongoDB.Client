using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        public static readonly TypeSyntax BsonReaderType = SF.ParseTypeName("MongoDB.Client.Bson.Reader.BsonReader");
        public static readonly TypeSyntax BsonWriterType = SF.ParseTypeName("MongoDB.Client.Bson.Writer.BsonWriter");
        public static NameSyntax DictionaryOfStringStringName = GenericName(Identifier("Dictionary"), StringPredefinedType(), StringPredefinedType());
        public static NameSyntax IReadOnlyDictionaryOfStringStringName = GenericName(Identifier("IReadOnlyDictionary"), StringPredefinedType(), StringPredefinedType());
        public static SyntaxToken MappingToken => Identifier($"__MAPPING__");
        public static ExpressionSyntax BsonTypeNull => SimpleMemberAccess(Identifier("BsonType"), Identifier("Null"));
        public static SyntaxToken TrySkipLabel => Identifier("TRY_SKIP_LABEL");
        public static SyntaxToken NullableHasValueToken => Identifier("HasValue");
        public static SyntaxToken NullableValueToken => Identifier("Value");
        public static SyntaxToken CollectionAddToken => Identifier("Add");
        public static SyntaxToken ListCountToken => Identifier("Count");

        public static SyntaxToken WriteBsonToken => Identifier("WriteBson");
        public static SyntaxToken TryParseBsonToken => Identifier("TryParseBson");
        public static SyntaxToken BsonReaderToken => Identifier("reader");
        public static SyntaxToken BsonWriterToken => Identifier("writer");
        public static SyntaxToken TryParseOutVarToken => Identifier("message");
        public static SyntaxToken WriterInputVarToken => Identifier("message");
        public static SyntaxToken BsonNameToken => Identifier("bsonName");
        public static SyntaxToken BsonTypeToken => Identifier("bsonType");

        public static SyntaxToken ListDocLenToken => Identifier("listDocLength");
        public static SyntaxToken ListUnreadedToken => Identifier("listUnreaded");
        public static SyntaxToken ListEndMarkerToken => Identifier("listEndMarker");
        public static SyntaxToken ListBsonTypeToken => Identifier("listBsonType");
        public static SyntaxToken ListBsonNameToken => Identifier("listBsonName");
        public static SyntaxToken ListToken => Identifier("list");
        public static SyntaxToken TempToken => Identifier("temp");
        public static SyntaxToken InternalListToken => Identifier("internalList");

        public static SyntaxToken DictionaryDocLenToken => Identifier("dictionaryDocLength");
        public static SyntaxToken DictionaryUnreadedToken => Identifier("dictionaryUnreaded");
        public static SyntaxToken DictionaryEndMarkerToken => Identifier("dictionaryEndMarker");
        public static SyntaxToken DictionaryBsonTypeToken => Identifier("dictionaryBsonType");
        public static SyntaxToken DictionaryBsonNameToken => Identifier("dictionaryBsonName");
        public static SyntaxToken DictionaryToken => Identifier("dictionary");
        public static SyntaxToken InternalDictionaryToken => Identifier("internalDictionary");
    }
}
