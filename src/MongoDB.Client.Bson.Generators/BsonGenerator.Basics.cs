using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        public static TypeSyntax VarType => SF.ParseTypeName("var");
        public static ExpressionSyntax NewBsonObjectIdExpr => InvocationExpr(TypeFullName(BsonObjectId), SF.IdentifierName("NewObjectId"));
        public static ExpressionSyntax BsonNameLengthExpr => SimpleMemberAccess(BsonNameToken, Identifier("Length"));
        public static SyntaxToken BsonNameLengthToken => Identifier("bsonNameLength");
        public static GenericNameSyntax ReadOnlySpanByteName => GenericName(Identifier("ReadOnlySpan"), BytePredefinedType());
        public static GenericNameSyntax SpanByteName => GenericName(Identifier("Span"), BytePredefinedType());
        public static ContinueStatementSyntax ContinueStatement => SF.ContinueStatement();
        public static StatementSyntax ReturnTrueStatement => ReturnStatement(SF.LiteralExpression(SyntaxKind.TrueLiteralExpression));
        public static StatementSyntax ReturnFalseStatement => ReturnStatement(SF.LiteralExpression(SyntaxKind.FalseLiteralExpression));
        public static StatementSyntax ReturnNothingStatement => ReturnStatement();
        public static SizeOfExpressionSyntax SizeOfInt32Expr => SizeOf(IntPredefinedType());
        public static BreakStatementSyntax BreakStatement => SF.BreakStatement();
        public static SyntaxToken SequenceEqualToken => SF.Identifier("SequenceEqual");
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
            if (original is IArrayTypeSymbol arraySym && arraySym.NullableAnnotation == NullableAnnotation.Annotated)
            {
                //var extractedElementType = ExtractTypeFromNullableIfNeed(arraySym.ElementType);
                //return BsonSerializerGenerator.Compilation.CreateArrayTypeSymbol(extractedElementType, 1);
                return Compilation.CreateArrayTypeSymbol(arraySym.ElementType, 1);
            }

            return original;
        }
        public static ExpressionSyntax SpanSequenceEqual(SyntaxToken spanName, SyntaxToken otherSpanName, int aliasNameLength)
        {
            //var equalNum = aliasNameLength switch
            //{
            //    < 5 => aliasNameLength,
            //    < 8 => 5,
            //    8 => 8,
            //    < 16 => 9,
            //    16 => 16,
            //    < 32 => 17,
            //    32 => 32,
            //    < 64 => 33,
            //    64 => 64,
            //    _ => 65
            //};
            //return InvocationExpr(IdentifierName(spanName), SF.Identifier("SequenceEqual" + equalNum), SF.Argument(IdentifierName(otherSpanName)));
            return InvocationExpr(IdentifierName(spanName), SequenceEqualToken, SF.Argument(IdentifierName(otherSpanName)));
        }
        public static readonly LiteralExpressionSyntax TrueLiteralExpr = SF.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        public static readonly LiteralExpressionSyntax FalseLiteralExpr = SF.LiteralExpression(SyntaxKind.FalseLiteralExpression);
        public static StatementSyntax[] Statements(params ExpressionSyntax[] expressions)
        {
            return expressions.Select(expr => Statement(expr)).ToArray();
        }
        public static StatementSyntax[] Statements(List<ExpressionSyntax> expressions)
        {
            return expressions.Select(expr => Statement(expr)).ToArray();
        }
        public static StatementSyntax[] Statements(params StatementSyntax[] statements)
        {
            return statements;
        }
        public static StatementSyntax[] Statements(StatementSyntax first, params StatementSyntax[] statements)
        {
            var array = new StatementSyntax[statements.Length + 1];
            array[0] = first;

            for (int i = 0; i < statements.Length; i++)
            {
                array[i + 1] = statements[i];
            }

            return array;
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
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, SimpleNameSyntax member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, GenericNameSyntax member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, SyntaxToken member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, IdentifierName(member));
        }
        public static MemberAccessExpressionSyntax SimpleMemberAccess(SyntaxToken source, SyntaxToken member)
        {
            return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(source), IdentifierName(member));
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
        public static ExpressionStatementSyntax InvocationExprStatement(SyntaxToken source, SyntaxToken member, params ArgumentSyntax[] args)
        {
            return SF.ExpressionStatement(InvocationExpr(source, member, args));
        }
        public static ExpressionStatementSyntax InvocationExprStatement(ExpressionSyntax source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.ExpressionStatement(InvocationExpr(source, member, args));
        }
        public static ExpressionStatementSyntax InvocationExprStatement(ExpressionSyntax source, SyntaxToken member, params ArgumentSyntax[] args)
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
        public static ExpressionSyntax InvocationExpr(SyntaxToken source, SyntaxToken member, params ArgumentSyntax[] args)
        {
            return InvocationExpr(IdentifierName(source), IdentifierName(member), args);
        }
        public static ExpressionSyntax InvocationExpr(SyntaxToken source, GenericNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(IdentifierName(source), member), SF.ArgumentList().AddArguments(args));
        }
        public static ExpressionSyntax InvocationExpr(ExpressionSyntax source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static ExpressionSyntax InvocationExpr(ExpressionSyntax source, SyntaxToken member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static ExpressionSyntax InvocationExpr(SyntaxToken source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static ExpressionSyntax InvocationExpr(ISymbol source, SyntaxToken member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(IdentifierName(source), IdentifierName(member)), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static InvocationExpressionSyntax InvocationExpr(ExpressionSyntax source, SimpleNameSyntax member, params ArgumentSyntax[] args)
        {
            return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
        }
        public static ExpressionSyntax InvocationExpr(IdentifierNameSyntax member, params ArgumentSyntax[] args)
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
        public static ArgumentSyntax OutArgument(SyntaxToken token, NameColonSyntax colonName = default)
        {
            return OutArgument(IdentifierName(token), colonName);
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
        public static ParameterSyntax OutParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: SyntaxTokenList(SF.Token(SyntaxKind.OutKeyword)),
                identifier: identifier,
                type: type,
                @default: @default);
        }
        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: default,
                identifier: identifier,
                type: type,
                @default: @default);
        }
        public static ParameterSyntax InParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
        {
            return SF.Parameter(
                attributeLists: default,
                modifiers: SyntaxTokenList(SF.Token(SyntaxKind.InKeyword)),
                identifier: identifier,
                type: type,
                @default: @default);
        }
        public static ParameterSyntax RefParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
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
        public static PredefinedTypeSyntax NativeIntPredefinedType()
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
        public static LiteralExpressionSyntax Utf8StringLiteralExpression(string value)
        {
            return SF.LiteralExpression(SyntaxKind.Utf8StringLiteralExpression, SF.ParseToken($"\"{value}\"u8"));
        }
        public static IdentifierNameSyntax IdentifierName(SyntaxToken token)
        {
            return SF.IdentifierName(token);
        }
        public static IdentifierNameSyntax IdentifierName(string name)
        {
            return SF.IdentifierName(name);
        }
        public static SyntaxToken Identifier(string name)
        {
            return SF.Identifier(name);
        }
        public static IdentifierNameSyntax IdentifierName(ISymbol sym)
        {
            return SF.IdentifierName(sym.Name);
        }
        public static IdentifierNameSyntax IdentifierFullName(ISymbol sym)
        {
            return IdentifierName(sym.ToString());
        }

        public static SyntaxToken TokenName(ISymbol sym)
        {
            return Identifier(sym.Name);
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
            return Identifier(sym.ToString());
        }
        public static TypeSyntax TypeFullName(ISymbol sym)
        {
            //TODO: FIX THIS SHIT
            ISymbol trueType = sym.Name.Equals("Nullable") ? ((INamedTypeSymbol)sym).TypeParameters[0] : sym;

            return SF.ParseTypeName(trueType.ToString());
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
        public static ExpressionStatementSyntax SimpleAssignExprStatement(ExpressionSyntax left, SyntaxToken right)
        {
            return SF.ExpressionStatement(SimpleAssignExpr(left, right));
        }
        public static AssignmentExpressionSyntax SimpleAssignExpr(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
        }
        public static AssignmentExpressionSyntax SimpleAssignExpr(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(left), right);
        }
        public static AssignmentExpressionSyntax SimpleAssignExpr(ExpressionSyntax left, SyntaxToken right)
        {
            return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, IdentifierName(right));
        }
        public static ArrayCreationExpressionSyntax SingleDimensionArrayCreation(TypeSyntax arrayType, int size, InitializerExpressionSyntax initializer = default)
        {
            var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));

            return SF.ArrayCreationExpression(SF.ArrayType(arrayType, rank), initializer);
        }

        public static StackAllocArrayCreationExpressionSyntax StackAllocByteArray(int size)
        {
            var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));

            return SF.StackAllocArrayCreationExpression(SF.ArrayType(BytePredefinedType(), rank));
        }
        public static DeclarationExpressionSyntax VarValueTupleDeclarationExpr(params SyntaxToken[] tokens)
        {
            var designation = SF.ParenthesizedVariableDesignation(new SeparatedSyntaxList<VariableDesignationSyntax>().AddRange(tokens.Select(SF.SingleVariableDesignation)));
            return VarVariableDeclarationExpr(designation);
        }
        public static CastExpressionSyntax CastToInt(ExpressionSyntax expr)
        {
            return SF.CastExpression(IntPredefinedType(), expr);
        }
        public static CastExpressionSyntax CastToNInt(ExpressionSyntax expr)
        {
            return SF.CastExpression(IdentifierName("nint"), expr);
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
        public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>() where T : SyntaxNode
        {
            return SF.SeparatedList<T>();
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
            return IfNotReturn(condition, ReturnFalseStatement);
        }
        public static IfStatementSyntax IfNotReturnFalseElse(ExpressionSyntax condition, ExpressionSyntax elseClause)
        {
            return SF.IfStatement(
                SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
                SF.Block(ReturnFalseStatement),
                SF.ElseClause(SF.Block(SF.ExpressionStatement(elseClause))));
        }
        public static IfStatementSyntax IfNotReturnFalseElse(ExpressionSyntax condition, BlockSyntax @else)
        {
            //return IfNotReturn(condition, SF.ReturnStatement(FalseLiteralExpr()));
            return SF.IfStatement(
                SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
                SF.Block(ReturnFalseStatement),
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
        public static IfStatementSyntax IfGoto(ExpressionSyntax condition, SyntaxToken gotoLabel)
        {
            return SF.IfStatement(condition, SF.Block(Goto(gotoLabel)));
        }
        public static IfStatementSyntax IfBreak(ExpressionSyntax condition)
        {
            return SF.IfStatement(condition, SF.Block(BreakStatement));
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

        public static BinaryExpressionSyntax BinaryExprMinus(ExpressionSyntax left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.SubtractExpression, left, right);
        }
        public static BinaryExpressionSyntax BinaryExprMinus(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.SubtractExpression, IdentifierName(left), right);
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
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(SyntaxToken left, SyntaxToken right)
        {
            return BinaryExprEqualsEquals(IdentifierName(left), IdentifierName(right));
        }
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(ExpressionSyntax left, SyntaxToken right)
        {
            return SF.BinaryExpression(SyntaxKind.EqualsExpression, left, IdentifierName(right));
        }

        public static ExpressionSyntax AddAssignmentExpr(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.AssignmentExpression(SyntaxKind.AddAssignmentExpression, IdentifierName(left), right);
        }
        public static BinaryExpressionSyntax BinaryExprEqualsEquals(SyntaxToken left, ExpressionSyntax right)
        {
            return SF.BinaryExpression(SyntaxKind.EqualsExpression, IdentifierName(left), right);
        }
        public static DeclarationExpressionSyntax VarVariableDeclarationExpr(SyntaxToken varId)
        {
            return SF.DeclarationExpression(VarType, SF.SingleVariableDesignation(varId));
        }
        public static DeclarationExpressionSyntax VarVariableDeclarationExpr(VariableDesignationSyntax designator)
        {
            return SF.DeclarationExpression(VarType, designator);
        }
        public static DeclarationExpressionSyntax IntVariableDeclarationExpr(SyntaxToken varId)
        {
            return SF.DeclarationExpression(IntPredefinedType(), SF.SingleVariableDesignation(varId));
        }
        public static DeclarationExpressionSyntax LongVariableDeclarationExpr(SyntaxToken varId)
        {
            return SF.DeclarationExpression(LongPredefinedType(), SF.SingleVariableDesignation(varId));
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

        public static StatementSyntax ForStatement(ExpressionSyntax condition, ExpressionSyntax incrementor, BlockSyntax body)
        {
            return SF.ForStatement(
                declaration: default,
                initializers: SeparatedList<ExpressionSyntax>(),
                condition: condition,
                incrementors: SeparatedList(incrementor),
                statement: body
            );
        }
        public static ForEachStatementSyntax ForEachStatement(SyntaxToken identifier, ExpressionSyntax expression, BlockSyntax body)
        {
            return SF.ForEachStatement(VarType, identifier, expression, body);
        }
        public static ForEachVariableStatementSyntax ForEachVariableStatement(ExpressionSyntax variable, ExpressionSyntax expression, BlockSyntax body)
        {
            return SF.ForEachVariableStatement(variable, expression, body);
        }
        public static ElementAccessExpressionSyntax ElementAccessExpr(ExpressionSyntax target, SyntaxToken index)
        {
            return SF.ElementAccessExpression(target, SF.BracketedArgumentList(SeparatedList(SF.Argument(IdentifierName(index)))));
        }
        public static ElementAccessExpressionSyntax ElementAccessExpr(SyntaxToken target, ExpressionSyntax index)
        {
            return SF.ElementAccessExpression(IdentifierName(target), SF.BracketedArgumentList(SeparatedList(SF.Argument(index))));
        }
        public static ExpressionSyntax ElementAccessExpr(SyntaxToken target, SyntaxToken index)
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
        public static BlockSyntax Block(LocalDeclarationStatementSyntax expr, StatementSyntax[] statements)
        {
            return SF.Block(expr).AddStatements(statements);
        }

        public static BlockSyntax Block(StatementSyntax first, ImmutableList<StatementSyntax>.Builder buider)
        {
            return SF.Block(first).AddStatements(buider.ToArray());
        }
        public static BlockSyntax Block(List<StatementSyntax> satatements)
        {
            return Block(satatements.ToArray());
        }
        public static BlockSyntax Block(ImmutableList<StatementSyntax>.Builder builder, ExpressionSyntax expr)
        {
            return Block(builder.ToArray(), expr);
        }
        public static BlockSyntax Block(StatementSyntax[] statements, StatementSyntax statement)
        {
            return SF.Block(statements).AddStatements(statement);
        }
        public static BlockSyntax Block(StatementSyntax[] statements, ExpressionSyntax expr)
        {
            return SF.Block(statements).AddStatements(Statement(expr));
        }
        public static BlockSyntax Block(ExpressionStatementSyntax expr, StatementSyntax[] statements1, params StatementSyntax[] statements2)
        {
            return SF.Block(expr).AddStatements(statements1).AddStatements(statements2);
        }
        public static BlockSyntax Block(IfStatementSyntax if1, IfStatementSyntax if2, IfStatementSyntax if3, StatementSyntax[] statements1, params StatementSyntax[] statements2)
        {
            return SF.Block(if1, if2, if3).AddStatements(statements1).AddStatements(statements2);
        }
        public static BlockSyntax Block(IfStatementSyntax if1, IfStatementSyntax if2, IfStatementSyntax if3, ImmutableList<StatementSyntax>.Builder builder)
        {
            return SF.Block(if1, if2, if3).AddStatements(builder.ToArray());
        }

        public static BlockSyntax Block(params ExpressionSyntax[] expressions)
        {
            return SF.Block(expressions.Select(e => Statement(e)));
        }
        public static BlockSyntax Block(ExpressionSyntax expression, params StatementSyntax[] statements)
        {
            return SF.Block(Statement(expression)).AddStatements(statements);
        }
        public static BlockSyntax Block(ExpressionSyntax expr1, ExpressionSyntax expr2, StatementSyntax statement)
        {
            return SF.Block(Statement(expr1), Statement(expr2), statement);
        }
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
        public static NameColonSyntax NameColon(string name)
        {
            return SF.NameColon(IdentifierName(name));
        }
        public static LabeledStatementSyntax Label(SyntaxToken identifier, StatementSyntax lowerStatement)
        {
            return SF.LabeledStatement(identifier, lowerStatement);
        }
        public static GotoStatementSyntax Goto(SyntaxToken identifier)
        {
            return SF.GotoStatement(SyntaxKind.GotoStatement, IdentifierName(identifier));
        }
        public static SwitchSectionSyntax SwitchSection(ExpressionSyntax labelExpr, BlockSyntax body)
        {
            var label = new SyntaxList<SwitchLabelSyntax>(SF.CaseSwitchLabel(labelExpr));
            return SF.SwitchSection(label, new SyntaxList<StatementSyntax>(body));
        }
        public static SwitchStatementSyntax SwitchStatement(SyntaxToken token, ImmutableList<SwitchSectionSyntax>.Builder sections)
        {
            return SF.SwitchStatement(IdentifierName(token), new SyntaxList<SwitchSectionSyntax>(sections.ToArray()));
        }
        public static SwitchStatementSyntax SwitchStatement(ExpressionSyntax expr, ImmutableList<SwitchSectionSyntax>.Builder sections)
        {
            return SF.SwitchStatement(expr, new SyntaxList<SwitchSectionSyntax>(sections.ToArray()));
        }
        public static SwitchStatementSyntax SwitchStatement(ExpressionSyntax expr, List<SwitchSectionSyntax> sections)
        {
            return SF.SwitchStatement(expr, new SyntaxList<SwitchSectionSyntax>(sections.ToArray()));
        }
    }
}
