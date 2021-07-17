using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly TypeSyntax BsonReaderType = SF.ParseTypeName("MongoDB.Client.Bson.Reader.BsonReader");
        public static readonly TypeSyntax BsonWriterType = SF.ParseTypeName("MongoDB.Client.Bson.Writer.BsonWriter");
        public static SyntaxToken TrySkipLabel => Identifier("TRY_SKIP_LABEL");
        public static SyntaxToken NullableHasValueToken => Identifier("HasValue");
        public static SyntaxToken NullableValueToken => Identifier("Value");
        public static SyntaxToken CollectionAddToken => Identifier("Add");
        public static SyntaxToken ListCountToken => Identifier("Count");

        public static SyntaxToken WriteBsonToken => Identifier("WriteBson");
        public static SyntaxToken CreateMessageToken => Identifier("CreateMessage");
        public static SyntaxToken TryParseBsonToken => Identifier("TryParseBson");
        public static SyntaxToken BsonReaderToken => Identifier("reader");
        public static SyntaxToken BsonWriterToken => Identifier("writer");
        public static SyntaxToken TryParseOutVarToken => Identifier("message");
        public static SyntaxToken WriterInputVarToken => Identifier("message");
        public static SyntaxToken BsonNameToken => Identifier("bsonName");
        public static SyntaxToken BsonTypeToken => Identifier("bsonType");
    }
}
