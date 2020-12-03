using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static StatementSyntax[] Statements(params ExpressionSyntax[] expressions)
        {
            return expressions.Select( expr => Statement(expr) ).ToArray();
        }
        public static StatementSyntax[] Statements(params StatementSyntax[] statements)
        {
            return statements;
        }
        public static StatementSyntax Statement(ExpressionSyntax expr)
        {
            return SF.ExpressionStatement(expr);
        }
        public static ObjectCreationExpressionSyntax ObjectCreation(TypeSyntax type, params ArgumentSyntax[] args)
        {
            return SF.ObjectCreationExpression(type, args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args), default);
        }
        public static ObjectCreationExpressionSyntax ObjectCreation(INamedTypeSymbol sym, params ArgumentSyntax[] args)
        {
            ITypeSymbol trueType = sym.Name.Equals("Nullable") ? sym.TypeParameters[0] : sym;
            return SF.ObjectCreationExpression(SF.ParseTypeName(trueType.ToString()), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args), default);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, IdentifierNameSyntax member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(SyntaxToken source, IdentifierNameSyntax member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(source), member);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, IdentifierNameSyntax member1, IdentifierNameSyntax member2)
        {
            var first = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member1);
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, first, member2);
        }
        public static ExpressionStatementSyntax InvocationExprStatement(ExpressionSyntax source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.ExpressionStatement(InvocationExpr(source, member, args));
        }
        public static ExpressionStatementSyntax InvocationExprStatement(SyntaxToken source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.ExpressionStatement(InvocationExpr(IdentifierName(source), member, args));
        }
        public static ExpressionStatementSyntax InvocationExprStatement(IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.ExpressionStatement(InvocationExpr(member, args));
        }
        public static ExpressionStatementSyntax InvocationExprStatement(SyntaxToken member, params ArgumentSyntax[] args)
        {
            return SF.ExpressionStatement(InvocationExpr(IdentifierName(member), args));
        }
        public static InvocationExpressionSyntax InvocationExpr(ExpressionSyntax source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static InvocationExpressionSyntax InvocationExpr(IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(member, args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
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
        public static ArgumentSyntax Argument(SyntaxToken token, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, default, IdentifierName(token));
        }
        public static ArgumentSyntax Argument(ExpressionSyntax expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, default, expr);
        }
        public static ArgumentSyntax InArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.InKeyword), expr);
        }
        public static ArgumentSyntax InArgument(SyntaxToken expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.InKeyword), IdentifierName(expr));
        }
        public static ArgumentSyntax RefArgument(SyntaxToken token, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.RefKeyword), IdentifierName(token));
        }
        public static ArgumentSyntax RefArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
        {
            return SF.Argument(colonName, SF.Token(SyntaxKind.RefKeyword), expr);
        }
        public static ParameterSyntax OutParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax? @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: SyntaxTokenList(SF.Token(SyntaxKind.OutKeyword)),
                identifier: identifier,
                type: type,
                @default: @default);
        }
        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax? @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: default,
                identifier: identifier,
                type: type,
                @default: @default);
        }
        public static ParameterSyntax InParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax? @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: SyntaxTokenList(SF.Token(SyntaxKind.InKeyword)),
                identifier: identifier,
                type: type,
                @default: @default);
        }
        public static ParameterSyntax RefParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax? @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: SyntaxTokenList(SF.Token(SyntaxKind.RefKeyword)),
                identifier: identifier,
                type: type,
                @default: @default);
        }

        public static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters)
        {
            return SF.ParameterList().AddParameters(parameters);
        }
        public static SyntaxToken ByteKeyword()
        {
            return SF.Token(SyntaxKind.ByteKeyword);
        }
        public static SyntaxToken BoolKeyword()
        {
            return SF.Token(SyntaxKind.BoolKeyword);
        }
        public static SyntaxToken VoidKeyword()
        {
            return SF.Token(SyntaxKind.VoidKeyword);
        }
        public static SyntaxToken IntKeyword()
        {
            return SF.Token(SyntaxKind.IntKeyword);
        }
        public static SyntaxToken LongKeyword()
        {
            return SF.Token(SyntaxKind.LongKeyword);
        }
        public static SyntaxToken PublicKeyword()
        {
            return SF.Token(SyntaxKind.PublicKeyword);
        }
        public static SyntaxToken PrivateKeyword()
        {
            return SF.Token(SyntaxKind.PrivateKeyword);
        }
        public static SyntaxToken PartialKeyword()
        {
            return SF.Token(SyntaxKind.PartialKeyword);
        }
        public static SyntaxToken RecordKeyword()
        {
            return SF.Token(SyntaxKind.RecordKeyword);
        }
        public static SyntaxToken StaticKeyword()
        {
            return SF.Token(SyntaxKind.StaticKeyword);
        }
        public static SyntaxToken SealedKeyword()
        {
            return SF.Token(SyntaxKind.SealedKeyword);
        }
        public static SyntaxToken ReadOnlyKeyword()
        {
            return SF.Token(SyntaxKind.ReadOnlyKeyword);
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
        public static PredefinedTypeSyntax BoolPredefinedType()
        {
            return SF.PredefinedType(BoolKeyword());
        }
        public static PredefinedTypeSyntax VoidPredefinedType()
        {
            return SF.PredefinedType(VoidKeyword());
        }
        public static LiteralExpressionSyntax DefaultLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression);
        }
        public static LiteralExpressionSyntax NullLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.NullLiteralExpression);
        }
        public static LiteralExpressionSyntax NumericLiteralExpr(int value)
        {
            return SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(value));
        }
        public static LiteralExpressionSyntax NumericLiteralExpr(byte value)
        {
            return SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(value));
        }
        public static LiteralExpressionSyntax CharacterLiteralExpr(char value)
        {
            return SF.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SF.Literal(value));
        }
        public static LiteralExpressionSyntax TrueLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        }
        public static LiteralExpressionSyntax FalseLiteralExpr()
        {
            return SF.LiteralExpression(SyntaxKind.FalseLiteralExpression);
        }
        public static IdentifierNameSyntax IdentifierName(SyntaxToken token)
        {
            return SF.IdentifierName(token);
        }
        public static IdentifierNameSyntax IdentifierName(string name)
        {
            return SF.IdentifierName(name);
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

        public static SyntaxToken SemicolonToken()
        {
            return SF.Token(SyntaxKind.SemicolonToken);
        }
        public static SyntaxToken OpenBraceToken()
        {
            return SF.Token(SyntaxKind.OpenBraceToken);
        }
        public static SyntaxToken CloseBraceToken()
        {
            return SF.Token(SyntaxKind.CloseBraceToken);
        }
        public static SyntaxToken TokenFullName(ISymbol sym)
        {
            return SF.Identifier(sym.ToString());
        }
        public static TypeSyntax TypeFullName(ISymbol sym)
        {
            ISymbol trueType = sym.Name.Equals("Nullable") ? ((INamedTypeSymbol)sym).TypeParameters[0] : sym;
            return SF.ParseTypeName(trueType.ToString());
        }
        public static TypeSyntax TypeName(ITypeSymbol sym)
        {
            ITypeSymbol trueType = sym.Name.Equals("Nullable") ? ((INamedTypeSymbol)sym).TypeParameters[0] : sym;
            return SF.ParseTypeName(trueType.Name);
        }
        public static TypeParameterSyntax FullTypeParameter(ITypeSymbol sym)
        {
            return SF.TypeParameter(sym.ToString());
        }
        public static TypeParameterSyntax TypeParameter(ITypeSymbol sym)
        {
            return SF.TypeParameter(sym.Name);
        }
        public static ExpressionStatementSyntax SimpleAssignExprStatement(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.ExpressionStatement(SimpleAssignExpr(left, right));
        }
        public static ExpressionStatementSyntax SimpleAssignExprStatement(SyntaxToken left, SyntaxToken right)
        {
            return SF.ExpressionStatement(SimpleAssignExpr(IdentifierName(left), IdentifierName(right)));
        }
        public static ExpressionStatementSyntax SimpleAssignExprStatement(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.ExpressionStatement(SimpleAssignExpr(IdentifierName(left), right));
        }
        public static AssignmentExpressionSyntax SimpleAssignExpr(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }
        public static ArrayCreationExpressionSyntax SingleDimensionArrayCreation(TypeSyntax arrayType, int size, InitializerExpressionSyntax? initializer = default)
        {
            var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));
            return SF.ArrayCreationExpression(SF.ArrayType(arrayType, rank), initializer);
        }

        public static StackAllocArrayCreationExpressionSyntax StackAllocByteArray(int size)
        {
            var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));
            return SF.StackAllocArrayCreationExpression(SF.ArrayType(BytePredefinedType(), rank));
        }
    }
}
