using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    static class Basics
    {
        public static string SerializerInterface => "IGenericBsonSerializer";
        public static SyntaxToken SerializerInterfaceIdentifier => SF.ParseToken("IGenericBsonSerializer");
        public static SyntaxToken ReaderInputVariable => SF.Identifier("reader");
        public static IdentifierNameSyntax ReaderInputVariableIdentifier => SF.IdentifierName("reader");
        public static TypeSyntax ReaderInputVariableType => SF.ParseTypeName("BsonReader");
        public static SyntaxToken TryParseOutputVariable => SF.Identifier("message");
        public static IdentifierNameSyntax TryParseOutVariableIdentifier => SF.IdentifierName("message");
        public static IdentifierNameSyntax TryParseBsonNameIdentifier => SF.IdentifierName("bsonName");
        public static IdentifierNameSyntax TryParseBsonTypeIdentifier => SF.IdentifierName("bsonType");
        public static SyntaxTokenList Public => new SyntaxTokenList().Add(SF.Token(SyntaxKind.PublicKeyword));
        public static SyntaxTokenList PrivateStatic => new SyntaxTokenList().Add(SF.Token(SyntaxKind.PrivateKeyword)).Add(SF.Token(SyntaxKind.StaticKeyword));
        public static LiteralExpressionSyntax NumberLiteral(int number)
        {
            return SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.ParseToken(number.ToString()));
        }
        public static string GenerateSerializerName(INamedTypeSymbol classSymbol)
        {
            return $"{classSymbol.Name}SerializerGenerated";
        }
        public static string GenerateSerializerNameStaticField(INamedTypeSymbol classSymbol)
        {
            return $"{GenerateSerializerName(classSymbol)}StaticField";
        }
        public static SyntaxToken GenerateReadOnlySpanNameToken(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.Identifier($"{classSymbol.Name}{memberdecl.StringFieldNameAlias}");
        }
        public static string GenerateReadOnlySpanName(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return $"{classSymbol.Name}{memberdecl.StringFieldNameAlias}";
        }
        public static SyntaxToken GenerateReadOnlySpanNameSyntaxToken(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.Identifier($"{classSymbol.Name}{memberdecl.StringFieldNameAlias}");
        }
        public static IdentifierNameSyntax GenerateReadOnlySpanNameIdentifier(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.IdentifierName($"{classSymbol.Name}{memberdecl.StringFieldNameAlias}");
        }

        public static MemberAccessExpressionSyntax SimpleMemberAccess(IdentifierNameSyntax source, IdentifierNameSyntax member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(classSymbol.Name), SF.IdentifierName(memberdecl.DeclSymbol.Name));
        }
        public static IdentifierNameSyntax IdentifierName(INamedTypeSymbol sym) => SF.IdentifierName(sym.Name);
        public static IdentifierNameSyntax IdentifierName(ISymbol sym) => SF.IdentifierName(sym.Name);
        public static ArgumentListSyntax Arguments(params IdentifierNameSyntax[] args)
        {

            return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().AddRange(args.AsEnumerable().Select(arg => SF.Argument(arg))));
        }
        public static ObjectCreationExpressionSyntax ObjectCreationWitoutArgs(INamedTypeSymbol sym) => SF.ObjectCreationExpression(SF.ParseTypeName(sym.Name), SF.ArgumentList(), default);
    }
}
