using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public static InvocationExpressionSyntax SpanSequenceEqual(ExpressionSyntax spanName, ExpressionSyntax otherSpanName)
        {
            return InvocationExpr(spanName, SF.IdentifierName("SequenceEqual"), SF.Argument(otherSpanName));
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
        public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>(T source) where T: SyntaxNode
        {
            return SF.SeparatedList(new []{source});
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
        public static IfStatementSyntax IfNotReturnFalseElse(ExpressionSyntax condition, ExpressionSyntax elseClause)
        {
            return SF.IfStatement(
                SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
                SF.Block(SF.ReturnStatement(FalseLiteralExpr())),
                SF.ElseClause(SF.Block(SF.ExpressionStatement(elseClause))));
        }
        public static IfStatementSyntax IfNotReturnFalseElse(ExpressionSyntax condition, BlockSyntax @else)
        {
            //return IfNotReturn(condition, SF.ReturnStatement(FalseLiteralExpr()));
            return SF.IfStatement(
                SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
                SF.Block(SF.ReturnStatement(FalseLiteralExpr())),
                SF.ElseClause(@else));
        }
        public static IfStatementSyntax IfContinue(ExpressionSyntax condition)
        {
            return SF.IfStatement(condition, SF.Block(SF.ContinueStatement()));
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
        public static SizeOfExpressionSyntax SizeOf(TypeSyntax type)
        {
            return SF.SizeOfExpression(type);
        }
        public static InvocationExpressionSyntax NameOf(ExpressionSyntax expr)
        {
            return SF.InvocationExpression(SF.IdentifierName("nameof"), SF.ArgumentList().AddArguments(SF.Argument(expr)));
        }
        public static SizeOfExpressionSyntax SizeOfInt()
        {
            return SizeOf(IntPredefinedType());
        }
        public static BinaryExpressionSyntax BinaryExprMinus(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.SubtractExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprLessThan(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.LessThanExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprPlus(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.AddExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprNotEquals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.EqualsExpression, left, right);
        }
        public static DeclarationExpressionSyntax VarVariableDeclarationExpr(SyntaxToken varId)
        {
            return SF.DeclarationExpression(SF.IdentifierName("var"), SF.SingleVariableDesignation(varId));
        }

        public static LocalDeclarationStatementSyntax VarLocalDeclarationStatement(SyntaxToken variable, ExpressionSyntax expression)
        {
            var declarator = SF.VariableDeclarator(variable, default, SF.EqualsValueClause(expression));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(SF.IdentifierName("var"), SeparatedList(declarator)));
        }
        public static LocalDeclarationStatementSyntax DefaultLocalDeclarationStatement(TypeSyntax type,  SyntaxToken variable)
        {
            var declarator = SF.VariableDeclarator(variable, default, SF.EqualsValueClause(DefaultLiteralExpr()));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(type, SeparatedList(declarator)));
        }
    }
}