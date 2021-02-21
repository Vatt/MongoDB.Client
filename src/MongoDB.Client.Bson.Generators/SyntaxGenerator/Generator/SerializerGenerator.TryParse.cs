using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static MethodDeclarationSyntax TryParseMethod(ContextCore ctx)
        {
            var decl = ctx.Declaration;
            var docLenToken = SF.Identifier("docLength");
            var unreadedToken = SF.Identifier("unreaded");
            var endMarkerToken = SF.Identifier("endMarker");
            var bsonTypeToken = SF.Identifier("bsonType");
            var bsonNameToken = SF.Identifier("bsonName");
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: new(PublicKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,//SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: SF.Identifier("TryParseBson"),
                    parameterList: ParameterList(RefParameter(ctx.BsonReaderType, ctx.BsonReaderToken),
                                                 OutParameter(TypeFullName(ctx.Declaration), ctx.TryParseOutVarToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                SF.Block(
                    SimpleAssignExprStatement(ctx.TryParseOutVar, DefaultLiteralExpr()))
                    .AddStatements(DeclareTempVariables(ctx)).AddStatements(
                    IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(docLenToken))),
                    VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemaining(), SizeOfInt())),
                      SF.WhileStatement(
                          condition:
                          BinaryExprLessThan(
                              BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                              BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                          statement:
                          SF.Block(
                              IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonTypeToken))),
                              IfNotReturnFalse(TryGetCStringAsSpan(VarVariableDeclarationExpr(bsonNameToken))))
                              .AddStatements(IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonTypeToken), NumericLiteralExpr(10))))
                              .AddStatements(Operations(ctx, bsonTypeToken, bsonNameToken))
                              .AddStatements(IfNotReturnFalse(TrySkip(IdentifierName(bsonTypeToken))))),
                      IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                      SF.IfStatement(
                          condition:
                              BinaryExprNotEquals(endMarkerToken, NumericLiteralExpr((byte)'\x00')),
                          statement: SF.Block(SF.ExpressionStatement(SerializerEndMarkerException(ctx.Declaration, IdentifierName(endMarkerToken))))))
                    .AddStatements(CreateMessage(ctx))
                    .AddStatements(SF.ReturnStatement(TrueLiteralExpr())));
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
                        args.Add(Argument(IdentifierName(member.AssignedVariable), SF.NameColon(IdentifierName(parameter.Name))));
                    }
                    else
                    {
                        assignments.Add(
                            SimpleAssignExprStatement(
                                SimpleMemberAccess(ctx.TryParseOutVar, IdentifierName(member.NameSym.Name)),
                                IdentifierName(member.AssignedVariable)));
                    }
                }

                var creation = SimpleAssignExprStatement(ctx.TryParseOutVar, ObjectCreation(ctx.Declaration, args.ToArray()));
                result.Add(creation);
                result.AddRange(assignments);
            }
            else
            {
                result.Add(SimpleAssignExprStatement(ctx.TryParseOutVar, ObjectCreation(ctx.Declaration)));
                foreach (var member in ctx.Members)
                {
                    result.Add(
                        SimpleAssignExprStatement(
                            SimpleMemberAccess(ctx.TryParseOutVar, IdentifierName(member.NameSym.Name)),
                            IdentifierName(member.AssignedVariable)));
                }
            }
            return result.ToArray();
        }
        private static StatementSyntax[] DeclareTempVariables(ContextCore ctx)
        {
            List<StatementSyntax> variables = new();
            foreach (var member in ctx.Members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                //if((trueType.IsReferenceType || trueType.TypeKind == TypeKind.Struct) && trueType.SpecialType == SpecialType.None)
                if (trueType.IsReferenceType)
                {
                    member.AssignedVariable = SF.Identifier($"{trueType.Name}{member.NameSym.Name}");
                    variables.Add(DefaultLocalDeclarationStatement(SF.ParseTypeName(trueType.ToString()), member.AssignedVariable));
                }
                else
                {
                    member.AssignedVariable = SF.Identifier($"{member.TypeSym.Name}{member.NameSym.Name}");
                    variables.Add(DefaultLocalDeclarationStatement(SF.ParseTypeName(member.TypeSym.ToString()), member.AssignedVariable));
                }

            }

            return variables.ToArray();
        }
        private static StatementSyntax[] Operations(ContextCore ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var decl = ctx.Declaration;
            var members = ctx.Members;
            List<StatementSyntax> statements = new();

            foreach (var member in members)
            {
                var trueType = ExtractTypeFromNullableIfNeed(member.TypeSym);
                if (trueType.TypeKind == TypeKind.Enum)
                {
                    var localReadEnumVar = SF.Identifier($"{member.AssignedVariable}EnumTemp");
                    int repr = GetEnumRepresentation(member.NameSym);
                    if (repr == -1) { repr = 2; }
                    if (repr != 1)
                    {
                        statements.Add(
                          SF.IfStatement(
                              condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                              statement: SF.Block(
                                  repr == 2 ?
                                    LocalDeclarationStatement(IntPredefinedType(), localReadEnumVar, DefaultLiteralExpr()) :
                                    LocalDeclarationStatement(LongPredefinedType(), localReadEnumVar, DefaultLiteralExpr()),
                                  repr == 2 ?
                                    IfNotReturnFalseElse(TryGetInt32(localReadEnumVar), SF.Block(SimpleAssignExprStatement(member.AssignedVariable, Cast(trueType, localReadEnumVar)))) :
                                    IfNotReturnFalseElse(TryGetInt64(localReadEnumVar), SF.Block(SimpleAssignExprStatement(member.AssignedVariable, Cast(trueType, localReadEnumVar)))),
                                  SF.ContinueStatement()
                            )));
                    }
                    else
                    {
                        var readMethod = IdentifierName(ReadStringReprEnumMethodName(ctx, member.TypeMetadata, member.NameSym));
                        statements.Add(
                            SF.IfStatement(
                                condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                                statement: SF.Block(
                                    IfNotReturnFalse(InvocationExpr(readMethod, RefArgument(ctx.BsonReaderToken), OutArgument(IdentifierName(member.AssignedVariable)))),
                                    SF.ContinueStatement()
                                )));
                    }
                }
                else
                {
                    var (operation, tempVar) = ReadOperation(ctx, member.NameSym, trueType, ctx.BsonReaderId, IdentifierName(member.AssignedVariable), bsonType);
                    if (operation != default)
                    {
                        statements.Add(
                            SF.IfStatement(
                                condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                                statement:
                                tempVar.HasValue
                                    ? SF.Block(IfNotReturnFalse(operation), SimpleAssignExprStatement(member.AssignedVariable, tempVar.Value), SF.ContinueStatement())
                                    : SF.Block(IfNotReturnFalse(operation), SF.ContinueStatement())
                            ));
                    }
                    else
                    {
                        if (IsBsonSerializable(trueType))
                        {
                            if (trueType.IsReferenceType)
                            {
                                var condition = InvocationExpr(IdentifierName(trueType.ToString()), IdentifierName("TryParseBson"), RefArgument(ctx.BsonReaderId), OutArgument(IdentifierName(member.AssignedVariable)));
                                statements.Add(
                                               SF.IfStatement(
                                                   condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                                                   statement:
                                                   SF.Block(IfNotReturnFalse(condition), SF.ContinueStatement())));
                            }
                            else
                            {
                                var localTryParseVar = Identifier($"{member.AssignedVariable.ToString()}TryParseTemp");
                                var condition = InvocationExpr(IdentifierName(trueType.ToString()), IdentifierName("TryParseBson"),
                                                               RefArgument(ctx.BsonReaderId), OutArgument(VarVariableDeclarationExpr(localTryParseVar)));
                                statements.Add(
                                               SF.IfStatement(
                                                   condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                                                   statement:
                                                   SF.Block(IfNotReturnFalse(condition), SimpleAssignExprStatement(member.AssignedVariable, localTryParseVar), SF.ContinueStatement())));
                            }

                        }
                        else
                        {
                            GeneratorDiagnostics.ReportSerializationMapUsingWarning(member.NameSym);
                            statements.Add(
                                           SF.IfStatement(
                                               condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                                               statement:
                                               SF.Block(
                                                   OtherTryParseBson(member))
                                               .AddStatements(
                                                   SF.ContinueStatement())));
                        }
                    }

                }

            }
            return statements.ToArray();
        }

        private static (ExpressionSyntax, SyntaxToken?) ReadOperation(ContextCore ctx, ISymbol nameSym, ITypeSymbol typeSym,
            ExpressionSyntax readerId, ExpressionSyntax readTarget, SyntaxToken bsonType)
        {

            if (ctx.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(typeSym.Name)) != default)
            {
                var temp = Identifier($"{nameSym.Name.ToString()}TempGenericNullable");
                if (typeSym.NullableAnnotation == NullableAnnotation.Annotated)
                {
                    return (TryReadGenericNullable(TypeName(typeSym.OriginalDefinition), bsonType, VarVariableDeclarationExpr(temp)), temp);
                }
                else
                {
                    return (TryReadGeneric(bsonType, readTarget), null);
                }
            }
            if (typeSym is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
            {
                if (TypeLib.IsListOrIList(namedType))
                {
                    return (InvocationExpr(IdentifierName(ReadArrayMethodName(nameSym, typeSym)), RefArgument(readerId), OutArgument(readTarget)), null);
                }
            }
            if (TryGetSimpleReadOperation(nameSym, typeSym, IdentifierName(bsonType), readTarget, out var simpleOperation))
            {
                return (simpleOperation, null);
            }
            return default;
        }
        private static bool TryGetSimpleReadOperation(ISymbol nameSym, ITypeSymbol typeSymbol, ExpressionSyntax bsonType, ExpressionSyntax variable, out InvocationExpressionSyntax expr)
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
            if (TypeLib.IsBsonDocument(typeSymbol))
            {
                expr = TryParseDocument(variable);
                return true;
            }
            if (TypeLib.IsBsonArray(typeSymbol))
            {
                expr = TryParseDocument(variable);
                return true;
            }
            if (TypeLib.IsGuid(typeSymbol))
            {
                expr = TryGetGuidWithBsonType(bsonType, variable);
                return true;
            }
            if (TypeLib.IsDateTimeOffset(typeSymbol))
            {
                expr = TryGetDateTimeWithBsonType(bsonType, variable);
                return true;
            }
            if (TypeLib.IsBsonObjectId(typeSymbol))
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