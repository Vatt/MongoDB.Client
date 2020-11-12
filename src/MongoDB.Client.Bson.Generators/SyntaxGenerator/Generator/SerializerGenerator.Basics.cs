using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {

        public static GenericNameSyntax ReadOnlySpanByte()
        {
            return GenericName(SF.Identifier("ReadOnlySpan"), BytePredefinedType());
        }
        public static GenericNameSyntax SpanByte()
        {
            return GenericName(SF.Identifier("Span"), BytePredefinedType());
        }
        public static SyntaxToken ReadOnlySpanNameSyntaxToken(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.Identifier($"{classSymbol.Name}{memberdecl.StringFieldNameAlias}");
        }
        public static IdentifierNameSyntax ReadOnlySpanNameIdentifier(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.IdentifierName($"{classSymbol.Name}{memberdecl.StringFieldNameAlias}");
        }
        public static CastExpressionSyntax CastToInt(ExpressionSyntax expr)
        {
            return SF.CastExpression(IntPredefinedType(), expr);
        }
        public static CastExpressionSyntax CastToLong(ExpressionSyntax expr)
        {
            return SF.CastExpression(LongPredefinedType(), expr);
        }

        public static IfStatementSyntax IfNotReturn(ExpressionSyntax condition, StatementSyntax returnStatement)
        {
            return SF.IfStatement(SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition), SF.Block(returnStatement));
        }

        public static IfStatementSyntax IfNotReturnFalse(ExpressionSyntax condition)
        {
            return IfNotReturn(condition, SF.ReturnStatement(FalseLiteralExpr()));
        }
    }
}