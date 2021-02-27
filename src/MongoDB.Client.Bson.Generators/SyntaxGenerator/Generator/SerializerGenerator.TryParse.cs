using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private readonly struct ReadOperationContext
        {
            public ExpressionSyntax Expr { get; }
            public SyntaxToken? TempVariable { get; }
            public ReadOperationContext(ExpressionSyntax expr)
            {
                Expr = expr;
                TempVariable = null;
            }
            public ReadOperationContext(ExpressionSyntax expr, SyntaxToken tempVar)
            {
                Expr = expr;
                TempVariable = tempVar;
            }
            public void Deconstruct(out ExpressionSyntax expr, out SyntaxToken? tempVar)
            {
                expr = Expr;
                tempVar = TempVariable;
            }
            public static implicit operator ReadOperationContext(ExpressionSyntax expr) => new ReadOperationContext(expr);
        }
        private static MethodDeclarationSyntax TryParseMethod(ContextCore ctx)
        {
            var decl = ctx.Declaration;
            var docLenToken = Identifier("docLength");
            var unreadedToken = Identifier("unreaded");
            var endMarkerToken = Identifier("endMarker");
            var bsonTypeToken = Identifier("bsonType");
            var bsonNameToken = Identifier("bsonName");
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: new(PublicKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,//SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: Identifier("TryParseBson"),
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 OutParameter(TypeFullName(ctx.Declaration), TryParseOutVarToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                    Block(
                     SimpleAssignExprStatement(TryParseOutVarToken, DefaultLiteralExpr()),
                     DeclareTempVariables(ctx),
                     IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(docLenToken))),
                     VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt)),
                       SF.WhileStatement(
                           condition:
                              BinaryExprLessThan(
                                  BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemainingExpr),
                                  BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                           statement:
                              Block(
                                  IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonTypeToken))),
                                  IfNotReturnFalse(TryGetCStringAsSpan(VarVariableDeclarationExpr(bsonNameToken))),
                                  IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonTypeToken), NumericLiteralExpr(10))),
                                  Operations(ctx, bsonTypeToken, bsonNameToken),
                                  IfNotReturnFalse(TrySkip(IdentifierName(bsonTypeToken))))),
                          IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                          SF.IfStatement(
                              condition:
                                  BinaryExprNotEquals(endMarkerToken, NumericLiteralExpr((byte)'\x00')),
                              statement: Block(SerializerEndMarkerException(ctx.Declaration, IdentifierName(endMarkerToken)))))
                        .AddStatements(CreateMessage(ctx))
                        .AddStatements(ReturnTrueStatement));
        }
        private static StatementSyntax[] DeclareTempVariables(ContextCore ctx)
        {
            ImmutableList<StatementSyntax>.Builder variables = ImmutableList.CreateBuilder<StatementSyntax>();
            foreach (var member in ctx.Members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (trueType.IsReferenceType)
                {
                    member.AssignedVariable = Identifier($"{trueType.Name}{member.NameSym.Name}");
                    variables.DefaultLocalDeclarationStatement(SF.ParseTypeName(trueType.ToString()), member.AssignedVariable);
                }
                else
                {
                    member.AssignedVariable = Identifier($"{member.TypeSym.Name}{member.NameSym.Name}");
                    variables.DefaultLocalDeclarationStatement(SF.ParseTypeName(member.TypeSym.ToString()), member.AssignedVariable);
                }

            }
            return variables.ToArray();
        }
        private static StatementSyntax[] CreateMessage(ContextCore ctx)
        {
            var result = new List<ExpressionStatementSyntax>();
            if (ctx.HavePrimaryConstructor)
            {
                List<ArgumentSyntax> args = new();
                var constructorParams = ctx.ConstructorParams;
                var assignments = new List<ExpressionStatementSyntax>();
                foreach (var member in ctx.Members)
                {
                    var parameter = constructorParams!.Value.FirstOrDefault(param => param.Name.Equals(member.NameSym.Name));
                    if (parameter != default)
                    {
                        args.Add(Argument(member.AssignedVariable, NameColon(parameter)));
                    }
                    else
                    {
                        assignments.Add(
                            SimpleAssignExprStatement(
                                SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                                IdentifierName(member.AssignedVariable)));
                    }
                }

                var creation = SimpleAssignExprStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration, args.ToArray()));
                result.Add(creation);
                result.AddRange(assignments);
            }
            else
            {
                result.Add(SimpleAssignExprStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration)));
                foreach (var member in ctx.Members)
                {
                    result.Add(
                        SimpleAssignExprStatement(
                            SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                            IdentifierName(member.AssignedVariable)));
                }
            }
            return result.ToArray();
        }
        private static StatementSyntax[] Operations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            foreach (var member in ctx.Members)
            {
                if (TryGenerateParseEnum(member.StaticSpanNameToken, member.AssignedVariable, member.NameSym, member.TypeSym, bsonName, builder))
                {
                    continue;
                }
                if (TryGenerateSimpleReadOperation(ctx, member, bsonType, bsonName, builder))
                {
                    continue;
                }
                if (TryGenerateTryParseBson(ctx, member, bsonName, builder))
                {
                    continue;
                }
                GeneratorDiagnostics.ReportSerializerMapUsingWarning(member.NameSym);
                builder.IfStatement(
                            condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken),
                            statement: Block(
                                        OtherTryParseBson(member),
                                        ContinueStatement));
            }
            return builder.ToArray();
        }

        private static bool TryGenerateParseEnum(SyntaxToken staticNameSpan, SyntaxToken readTarget, ISymbol nameSym, ITypeSymbol typeSym, SyntaxToken bsonName, ImmutableList<StatementSyntax>.Builder builder)
        {
            var trueType = ExtractTypeFromNullableIfNeed(typeSym);
            if (trueType.TypeKind != TypeKind.Enum)
            {
                return false;
            }
            var localReadEnumVar = Identifier($"{readTarget}EnumTemp");
            int repr = GetEnumRepresentation(nameSym);
            if (repr == -1) { repr = 2; }
            if (repr != 1)
            {
                builder.IfStatement(
                      condition: SpanSequenceEqual(bsonName, staticNameSpan),
                      statement: Block(
                          repr == 2 ?
                            IfNotReturnFalseElse(TryGetInt32(IntVariableDeclarationExpr(localReadEnumVar)), Block(SimpleAssignExprStatement(readTarget, Cast(trueType, localReadEnumVar)))) :
                            IfNotReturnFalseElse(TryGetInt64(LongVariableDeclarationExpr(localReadEnumVar)), Block(SimpleAssignExprStatement(readTarget, Cast(trueType, localReadEnumVar)))),
                            ContinueStatement
                    ));
            }
            else
            {
                var readMethod = IdentifierName(ReadStringReprEnumMethodName(trueType, nameSym));
                builder.IfStatement(
                        condition: SpanSequenceEqual(bsonName, staticNameSpan),
                        statement: Block(
                            IfNotReturnFalse(InvocationExpr(readMethod, RefArgument(BsonReaderToken), OutArgument(readTarget))),
                            ContinueStatement
                        ));
            }
            return true;
        }
        private static bool TryGenerateSimpleReadOperation(ContextCore ctx, MemberContext member, SyntaxToken bsonType, SyntaxToken bsonName, ImmutableList<StatementSyntax>.Builder builder)
        {
            var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
            var (operation, tempVar) = ReadOperation(ctx, member.NameSym, trueType, BsonReaderToken, IdentifierName(member.AssignedVariable), bsonType);
            if (operation != default)
            {
                builder.IfStatement(condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken),
                                    statement: tempVar.HasValue
                                        ? Block(IfNotReturnFalse(operation), SimpleAssignExprStatement(member.AssignedVariable, tempVar.Value), ContinueStatement)
                                        : Block(IfNotReturnFalse(operation), ContinueStatement));
                return true;
            }
            return false;
        }
        private static bool TryGenerateTryParseBson(ContextCore ctx, MemberContext member, SyntaxToken bsonName, ImmutableList<StatementSyntax>.Builder builder)
        {
            var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
            if (IsBsonSerializable(trueType))
            {
                if (trueType.IsReferenceType)
                {
                    var condition = InvocationExpr(IdentifierName(trueType.ToString()), IdentifierName("TryParseBson"),
                                                   RefArgument(BsonReaderToken),
                                                   OutArgument(member.AssignedVariable));
                    builder.IfStatement(condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken),
                                        statement: Block(IfNotReturnFalse(condition), ContinueStatement));
                    return true;
                }
                else
                {
                    var localTryParseVar = Identifier($"{member.AssignedVariable.ToString()}TryParseTemp");
                    var condition = InvocationExpr(IdentifierName(trueType.ToString()), IdentifierName("TryParseBson"),
                                                   RefArgument(BsonReaderToken), OutArgument(VarVariableDeclarationExpr(localTryParseVar)));

                    builder.IfStatement(condition: SpanSequenceEqual(bsonName, member.StaticSpanNameToken),
                                        statement:
                                            Block(
                                                IfNotReturnFalse(condition),
                                                SimpleAssignExprStatement(member.AssignedVariable, localTryParseVar),
                                                ContinueStatement));
                    return true;
                }
            }
            return false;
        }
        private static ReadOperationContext ReadOperation(ContextCore ctx, ISymbol nameSym, ITypeSymbol typeSym, SyntaxToken readerId, ExpressionSyntax readTarget, SyntaxToken bsonType)
        {
            if (ctx.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(typeSym.Name)) != default) // generic type arguments
            {
                var temp = Identifier($"{nameSym.Name}TempGenericNullable");
                if (typeSym.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return new(TryReadGenericNullable(TypeName(typeSym.OriginalDefinition), bsonType, VarVariableDeclarationExpr(temp)), temp);
                }
                else
                {
                    return TryReadGeneric(bsonType, readTarget);
                }
            }
            if (typeSym is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
            {
                if (IsListOrIList(namedType))
                {
                    return InvocationExpr(IdentifierName(ReadArrayMethodName(nameSym, typeSym)), RefArgument(readerId), OutArgument(readTarget));
                }
            }
            if (TryGetSimpleReadOperation(nameSym, typeSym, IdentifierName(bsonType), readTarget, out var simpleOperation))
            {
                return simpleOperation;
            }
            return default;
        }
        private static bool TryGetSimpleReadOperation(ISymbol nameSym, ITypeSymbol typeSymbol, ExpressionSyntax bsonType, ExpressionSyntax variable, out ExpressionSyntax expr)
        {
            expr = default;
            switch (typeSymbol.SpecialType)
            {
                case SpecialType.System_Double:
                    expr = TryGetDouble(variable);
                    return true;
                case SpecialType.System_String:
                    expr = TryGetString(variable);
                    return true;
                case SpecialType.System_Boolean:
                    expr = TryGetBoolean(variable);
                    return true;
                case SpecialType.System_Int32:
                    expr = TryGetInt32(variable);
                    return true;
                case SpecialType.System_Int64:
                    expr = TryGetInt64(variable);
                    return true;
                    //case SpecialType.System_DateTime:
                    //    expr = TryGetDateTimeWithBsonType(bsonType, variable);
                    //    return true;
            }
            if (IsBsonDocument(typeSymbol))
            {
                expr = TryParseDocument(variable);
                return true;
            }
            if (IsBsonArray(typeSymbol))
            {
                expr = TryParseDocument(variable);
                return true;
            }
            if (IsGuid(typeSymbol))
            {
                expr = TryGetGuidWithBsonType(bsonType, variable);
                return true;
            }
            if (IsDateTimeOffset(typeSymbol))
            {
                expr = TryGetDateTimeWithBsonType(bsonType, variable);
                return true;
            }
            if (IsBsonObjectId(typeSymbol))
            {
                expr = TryGetObjectId(variable);
                return true;
            }
            if (typeSymbol.SpecialType != SpecialType.None)
            {
                GeneratorDiagnostics.ReportUnsuporterTypeError(nameSym, typeSymbol);
            }
            return false;
        }
    }
}