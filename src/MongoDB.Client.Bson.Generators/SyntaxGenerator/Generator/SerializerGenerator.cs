using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static ObjectCreationExpressionSyntax ObjectCreation(INamedTypeSymbol sym, params ArgumentSyntax[] args)
        {
            return SF.ObjectCreationExpression(SF.ParseTypeName(sym.ToString()), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args), default);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(INamedTypeSymbol classSymbol, MemberDeclarationMeta memberdecl)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SF.IdentifierName(classSymbol.Name), SF.IdentifierName(memberdecl.DeclSymbol.Name));
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, MemberDeclarationMeta memberdecl)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, SF.IdentifierName(memberdecl.DeclSymbol.Name));
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, IdentifierNameSyntax member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, IdentifierNameSyntax member1, IdentifierNameSyntax member2)
        {
            var first = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member1);
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, first, member2);
        }
        public static InvocationExpressionSyntax InvocationExpr(ExpressionSyntax source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length ==0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static GenericNameSyntax GenericName(SyntaxToken name, params TypeSyntax[] types)
        {
            var typeList = SF.TypeArgumentList().AddArguments(types);
            return SF.GenericName(name, typeList);
        }

        public static ArgumentSyntax OutArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.OutKeyword), expr);
        }
        public static ArgumentSyntax InArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.InKeyword), expr);
        }
        public static ArgumentSyntax RefArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.RefKeyword), expr);
        }
        public static SyntaxToken ByteKeyword()
        {
            return SF.Token(SyntaxKind.ByteKeyword);
        }
        public static SyntaxToken IntKeyword()
        {
            return SF.Token(SyntaxKind.IntKeyword);
        }
        public static SyntaxToken LongKeyword()
        {
            return SF.Token(SyntaxKind.LongKeyword);
        }
        public static PredefinedTypeSyntax IntPredefinedType()
        {
            return SF.PredefinedType(IntKeyword());
        }
        public static PredefinedTypeSyntax LongPredefinedType()
        {
            return SF.PredefinedType(LongKeyword());
        }
        public static PredefinedTypeSyntax BytePredefinedType()
        {
            return SF.PredefinedType(ByteKeyword());
        }
        public static LiteralExpressionSyntax DefaultLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression);
        }
        public static LiteralExpressionSyntax NumerictLiteralExpr(int value)
        {
            return SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(value));
        }
        public static LiteralExpressionSyntax TrueLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        }
        public static LiteralExpressionSyntax FalseLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.FalseLiteralExpression);
        }
        public static IdentifierNameSyntax IdentifierName(ISymbol sym)
        {
            return SF.IdentifierName(sym.Name);
        }
        public static IdentifierNameSyntax IdentifierFullName(ISymbol sym)
        {
            return SF.IdentifierName(sym.ToString());
        }

        public static SyntaxToken TokenName(ISymbol sym)
        {
            return SF.Identifier(sym.Name);
        }
        public static SyntaxToken TokenFullName(ISymbol sym)
        {
            return SF.Identifier(sym.ToString());
        }
        public static TypeSyntax TypeFullName(ITypeSymbol sym)
        {
            return SF.ParseTypeName(sym.ToString());
        }
        public static TypeSyntax TypeName(ITypeSymbol sym)
        {
            return SF.ParseTypeName(sym.Name);
        }
        public static AssignmentExpressionSyntax SimpleAssignExpr(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }
    }
}
