using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static ITypeSymbol ExtractTypeFromNullableIfNeed(ITypeSymbol original)
        {
            if (original is INamedTypeSymbol namedType)
            {
                if (original.NullableAnnotation == NullableAnnotation.NotAnnotated || original.NullableAnnotation == NullableAnnotation.None)
                {
                    return original;
                }
                if (namedType.IsReferenceType)
                {
                    if (namedType.IsGenericType)
                    {
                        var constucted = namedType.OriginalDefinition.Construct(namedType.TypeArguments.ToArray());
                        return constucted;
                    }

                    return namedType.OriginalDefinition;
                }

                return namedType.TypeArguments[0];
            }
            return original;
        }

        public static readonly GenericNameSyntax ReadOnlySpanByte = GenericName(SF.Identifier("ReadOnlySpan"), BytePredefinedType());

        public static readonly GenericNameSyntax SpanByte = GenericName(SF.Identifier("Span"), BytePredefinedType());

        public static ExpressionSyntax SpanSequenceEqual(ExpressionSyntax spanName, ExpressionSyntax otherSpanName)
        {
            return InvocationExpr(spanName, SF.IdentifierName("SequenceEqual"), SF.Argument(otherSpanName));
        }

        public static readonly ExpressionSyntax NewBsonObjectId = InvocationExpr(TypeFullName(TypeLib.BsonObjectId), SF.IdentifierName("NewObjectId"));

        public static ExpressionSyntax SpanSequenceEqual(SyntaxToken spanName, SyntaxToken otherSpanName)
        {
            return InvocationExpr(IdentifierName(spanName), SF.IdentifierName("SequenceEqual"), SF.Argument(IdentifierName(otherSpanName)));
        }
        public static CastExpressionSyntax CastToInt(ExpressionSyntax expr)
        {
            return SF.CastExpression(IntPredefinedType(), expr);
        }

        public static CastExpressionSyntax Cast(TypeSyntax type, ExpressionSyntax expr)
        {
            return SF.CastExpression(type, expr);
        }
        public static CastExpressionSyntax Cast(ITypeSymbol type, ExpressionSyntax expr)
        {
            return SF.CastExpression(TypeFullName(type), expr);
        }
        public static CastExpressionSyntax Cast(ITypeSymbol type, SyntaxToken expr)
        {
            return SF.CastExpression(TypeFullName(type), IdentifierName(expr));
        }
        public static CastExpressionSyntax CastToLong(ExpressionSyntax expr)
        {
            return SF.CastExpression(LongPredefinedType(), expr);
        }

        public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>(IEnumerable<T> source) where T : SyntaxNode
        {
            return SF.SeparatedList(source);
        }
        public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>(T source) where T : SyntaxNode
        {
            return SF.SeparatedList(new[] { source });
        }

        public static SyntaxTokenList SyntaxTokenList(params SyntaxToken[] tokens)
        {
            return new(tokens);
        }
        public static IfStatementSyntax IfNotReturn(ExpressionSyntax condition, StatementSyntax returnStatement)
        {
            return SF.IfStatement(SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition), SF.Block(returnStatement));
        }
        public static IfStatementSyntax IfStatement(ExpressionSyntax condition, StatementSyntax statement, BlockSyntax @else)
        {
            return SF.IfStatement(condition, statement, SF.ElseClause(@else));
        }
        public static IfStatementSyntax IfStatement(ExpressionSyntax condition, StatementSyntax statement)
        {
            return SF.IfStatement(condition, statement);
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
        public static IfStatementSyntax IfNot(ExpressionSyntax condition, ExpressionSyntax statement)
        {
            return SF.IfStatement(
                SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
                SF.Block(SF.ExpressionStatement(statement)));
        }
        public static IfStatementSyntax IfNot(ExpressionSyntax condition, params StatementSyntax[] statement)
        {
            return SF.IfStatement(
                SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
                SF.Block(statement));
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

        public static DefaultExpressionSyntax Default(TypeSyntax type)
        {
            return SF.DefaultExpression(type);
        }

        public static InvocationExpressionSyntax NameOf(ExpressionSyntax expr)
        {
            return SF.InvocationExpression(SF.IdentifierName("nameof"), SF.ArgumentList().AddArguments(SF.Argument(expr)));
        }

        public static readonly SizeOfExpressionSyntax SizeOfInt = SizeOf(IntPredefinedType());

        public static BinaryExpressionSyntax BinaryExprMinus(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.SubtractExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprMinus(ExpressionSyntax left, SyntaxToken right)
        {
            return SF.BinaryExpression(SyntaxKind.SubtractExpression, left, IdentifierName(right));
        }
        public static BinaryExpressionSyntax BinaryExprLessThan(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.LessThanExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprLessThan(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.LessThanExpression, IdentifierName(left), right);
        }
        public static BinaryExpressionSyntax BinaryExprPlus(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.AddExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprNotEquals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.NotEqualsExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprNotEquals(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.NotEqualsExpression, IdentifierName(left), right);
        }
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.EqualsExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(ExpressionSyntax left, SyntaxToken right)
        {
            return SF.BinaryExpression(SyntaxKind.EqualsExpression, left, IdentifierName(right));
        }
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName(left), right);
        }
        public static DeclarationExpressionSyntax VarVariableDeclarationExpr(SyntaxToken varId)
        {
            return SF.DeclarationExpression(SF.IdentifierName("var"), SF.SingleVariableDesignation(varId));
        }
        public static DeclarationExpressionSyntax IntVariableDeclarationExpr(SyntaxToken varId)
        {
            return SF.DeclarationExpression(IntPredefinedType(), SF.SingleVariableDesignation(varId));
        }
        public static DeclarationExpressionSyntax TypedVariableDeclarationExpr(TypeSyntax type, SyntaxToken varId)
        {
            return SF.DeclarationExpression(type, SF.SingleVariableDesignation(varId));
        }
        public static VariableDeclarationSyntax VarVariableDeclaration(SyntaxToken variable, ExpressionSyntax expression)
        {
            var declarator = SF.VariableDeclarator(variable, default, SF.EqualsValueClause(expression));
            return SF.VariableDeclaration(SF.IdentifierName("var"), SeparatedList(declarator));
        }

        public static ForStatementSyntax ForStatement(SyntaxToken indexVar, ExpressionSyntax condition, ExpressionSyntax incrementor, BlockSyntax body)
        {
            return SF.ForStatement(
                declaration: VarVariableDeclaration(indexVar, NumericLiteralExpr(0)),
                initializers: new SeparatedSyntaxList<ExpressionSyntax>(),
                condition: condition,
                incrementors: SeparatedList(incrementor),
                statement: body
            );
        }

        public static ElementAccessExpressionSyntax ElementAccessExpr(ExpressionSyntax target, SyntaxToken index)
        {
            return SF.ElementAccessExpression(target, SF.BracketedArgumentList(SeparatedList(SF.Argument(IdentifierName(index)))));
        }
        public static ElementAccessExpressionSyntax ElementAccessExpr(SyntaxToken target, SyntaxToken index)
        {
            return SF.ElementAccessExpression(IdentifierName(target), SF.BracketedArgumentList(SeparatedList(SF.Argument(IdentifierName(index)))));
        }
        public static PostfixUnaryExpressionSyntax PostfixUnaryExpr(SyntaxToken variable)
        {
            return SF.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(variable));
        }
        public static LocalDeclarationStatementSyntax VarLocalDeclarationStatement(SyntaxToken variable, ExpressionSyntax expression)
        {
            var declarator = SF.VariableDeclarator(variable, default, SF.EqualsValueClause(expression));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(SF.IdentifierName("var"), SeparatedList(declarator)));
        }
        public static LocalDeclarationStatementSyntax LocalDeclarationStatement(TypeSyntax type, SyntaxToken variable, ExpressionSyntax expression)
        {
            var declarator = SF.VariableDeclarator(variable, default, SF.EqualsValueClause(expression));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(type, SeparatedList(declarator)));
        }
        public static LocalDeclarationStatementSyntax DefaultLocalDeclarationStatement(TypeSyntax type, SyntaxToken variable)
        {
            var declarator = SF.VariableDeclarator(variable, default, SF.EqualsValueClause(DefaultLiteralExpr()));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(type, SeparatedList(declarator)));
        }
        public static BlockSyntax Block(params StatementSyntax[] statements)
        {
            return SF.Block(statements);
        }
        public static BlockSyntax Block(ImmutableList<StatementSyntax>.Builder buiider)
        {
            return Block(buiider.ToArray());
        }
        public static BlockSyntax Block(StatementSyntax[] statements, StatementSyntax statement)
        {
            return SF.Block(statements).AddStatements(statement);
        }
        public static BlockSyntax Block(ExpressionStatementSyntax expr, StatementSyntax[] statements1, params StatementSyntax[] statements2)
        {
            return SF.Block(expr).AddStatements(statements1).AddStatements(statements2);
        }
        public static BlockSyntax Block(IfStatementSyntax if1, IfStatementSyntax if2, IfStatementSyntax if3, StatementSyntax[] statements1, params StatementSyntax[] statements2)
        {
            return SF.Block(if1, if2, if3).AddStatements(statements1).AddStatements(statements2);
        }
        public static BlockSyntax Block(params ExpressionSyntax[] expressions)
        {
            return SF.Block(expressions.Select(e => Statement(e)));
        }


        public static readonly ContinueStatementSyntax ContinueStatement = SF.ContinueStatement();

        public static ReturnStatementSyntax ReturnStatement(ExpressionSyntax expr = null)
        {
            return SF.ReturnStatement(expr);
        }
        public static NameColonSyntax NameColon(SyntaxToken name)
        {
            return SF.NameColon(IdentifierName(name));
        }
        public static NameColonSyntax NameColon(IdentifierNameSyntax name)
        {
            return SF.NameColon(name);
        }
        public static NameColonSyntax NameColon(ISymbol symbol)
        {
            return SF.NameColon(IdentifierName(symbol.Name));
        }
    }
}