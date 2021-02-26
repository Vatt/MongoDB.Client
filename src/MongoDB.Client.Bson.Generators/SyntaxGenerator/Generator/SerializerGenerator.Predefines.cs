using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static TypeSyntax BsonReaderType => SF.ParseTypeName("MongoDB.Client.Bson.Reader.BsonReader");
        public static TypeSyntax BsonWriterType => SF.ParseTypeName("MongoDB.Client.Bson.Writer.BsonWriter");

        public static SyntaxToken WriteBsonToken => Identifier("WriteBson");
        public static SyntaxToken TryParseToken => Identifier("TryParseBson");
        public static SyntaxToken NullableHasValueToken => Identifier("HasValue");
        public static SyntaxToken NullableValueToken => Identifier("Value");
        public static SyntaxToken BsonReaderToken => Identifier("reader");
        public static SyntaxToken BsonWriterToken => Identifier("writer");         
        public static SyntaxToken TryParseOutVarToken => Identifier("message");
        public static SyntaxToken WriterInputVarToken => Identifier("message");
    }
}