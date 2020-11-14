using System.Collections.Generic;
using System.Linq;
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
        //TODO: delete this
        public static SyntaxToken ReadOnlySpanNameSyntaxToken(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.Identifier($"{classSymbol.Name}{memberdecl.StringFieldNameAlias}");
        }
        //TODO: delete this
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

        public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>(IEnumerable<T> source) where T: SyntaxNode
        {
            return SF.SeparatedList(source);
        }

        public static SyntaxTokenList SyntaxTokenList(params SyntaxToken[] tokens)
        {
            return new(tokens);
        }
        public static IfStatementSyntax IfNotReturn(ExpressionSyntax condition, StatementSyntax returnStatement)
        {
            return SF.IfStatement(SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition), SF.Block(returnStatement));
        }

        public static IfStatementSyntax IfNotReturnFalse(ExpressionSyntax condition)
        {
            return IfNotReturn(condition, SF.ReturnStatement(FalseLiteralExpr()));
        }

        public static ArrayCreationExpressionSyntax SingleDimensionByteArrayCreation(int size, SeparatedSyntaxList<ExpressionSyntax>? expressions = default)
        {
            var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));
            if (expressions.HasValue)
            {
                return SF.ArrayCreationExpression(SF.ArrayType(BytePredefinedType(), rank), SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression, expressions.Value)); 
            }
            else
            {
                return SF.ArrayCreationExpression(SF.ArrayType(BytePredefinedType(), rank), SF.InitializerExpression(SyntaxKind.ArrayInitializerExpression));
            }
            
        }
    }
}