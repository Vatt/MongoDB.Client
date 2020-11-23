using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static MethodDeclarationSyntax TryParseMethod(ClassContext ctx)
        {
            var decl = ctx.Declaration;
            var docLenToken = SF.Identifier("docLength");
            var unreadedToken = SF.Identifier("unreaded");
            var endMarkerToken = SF.Identifier("endMarker");
            var bsonTypeToken = SF.Identifier("bsonType");
            var bsonNameToken = SF.Identifier("bsonName");
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: default,
                    explicitInterfaceSpecifier: SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: SF.Identifier("TryParse"),
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
                    IfNotReturnFalse(TryGetInt32(VarVariableDeclarationExpr(docLenToken))),
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
        private static StatementSyntax[] CreateMessage(ClassContext ctx)
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
        private static StatementSyntax[] DeclareTempVariables(ClassContext ctx)
        {
            List<StatementSyntax> variables = new();
            foreach (var member in ctx.Members)
            {
                member.AssignedVariable = SF.Identifier($"{member.TypeSym.Name}{member.NameSym.Name}");
                variables.Add(DefaultLocalDeclarationStatement(SF.ParseTypeName(member.TypeSym.ToString()), member.AssignedVariable));
            }

            return variables.ToArray();
        }
        private static StatementSyntax[] Operations(ClassContext ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var decl = ctx.Declaration;
            var members = ctx.Members;
            
            List<StatementSyntax> statements = new();
            
            foreach (var member in members)
            {
                if (member.TypeSym.TypeKind == TypeKind.Enum)
                {
                    var localReadEnumVar = SF.Identifier($"{member.AssignedVariable}Temp");
                    int repr = AttributeHelper.GetEnumRepresentation(member.NameSym);
                    if (repr == -1 ) { repr = 2; }
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
                                    IfNotReturnFalseElse(TryGetInt32(localReadEnumVar), SF.Block(SimpleAssignExprStatement(member.AssignedVariable, Cast(member.TypeSym, localReadEnumVar)))) :
                                    IfNotReturnFalseElse(TryGetInt64(localReadEnumVar), SF.Block(SimpleAssignExprStatement(member.AssignedVariable, Cast(member.TypeSym, localReadEnumVar)))),
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
                    var operation = ReadOperation(ctx, member.NameSym, member.TypeSym, ctx.BsonReaderId, IdentifierName(member.AssignedVariable), bsonType);
                    statements.Add(
                        SF.IfStatement(
                            condition: SpanSequenceEqual(bsonName, StaticFieldNameToken(member)),
                            statement: 
                            SF.Block(
                                    IfNotReturnFalse(operation),
                                    SF.ContinueStatement())));
                }

            }
            return statements.ToArray();
        }

        private static ExpressionSyntax ReadOperation(ClassContext ctx, ISymbol nameSym, ITypeSymbol typeSym, ExpressionSyntax readerId,
                                                      ExpressionSyntax readTarget, SyntaxToken bsonType)
        {
            ITypeSymbol trueType = typeSym.Name.Equals("Nullable") ? ((INamedTypeSymbol)typeSym).TypeParameters[0] : typeSym;

            if (TryGetSimpleReadOperation(trueType, IdentifierName(bsonType), readTarget, out var simpleOperation))
            {
                return simpleOperation;
            }

            if (ctx.GenericArgs?.FirstOrDefault(sym => sym.Name.Equals(trueType.Name)) != default)
            {
                return TryReadGeneric(bsonType, readTarget);
            }
            if (trueType is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
            {
                if (namedType.ToString().Contains("System.Collections.Generic.List") ||
                    namedType.ToString().Contains("System.Collections.Generic.IList"))
                //if (namedType.Equals(Types.List) || namedType.Equals(Types.IList))
                {
                    return InvocationExpr(IdentifierName(ReadArrayMethodName(nameSym, trueType)), RefArgument(readerId), OutArgument(readTarget));
                }
            }
            else
            {
                foreach (var context in ctx.Root.Contexts)
                {
                    //TODO: если сериализатор не из ЭТОЙ сборки, добавить ветку с мапой с проверкой на нуль
                    if (context.Declaration.ToString().Equals(trueType.ToString()))
                    {
                        return GeneratedSerializerTryParse(context, readerId, readTarget);
                    }
                }
            }

            return default;
        }
        private static bool TryGetSimpleReadOperation(ITypeSymbol typeSymbol, ExpressionSyntax bsonType, ExpressionSyntax variable, out InvocationExpressionSyntax expr)
        {
            expr = default;
            switch (typeSymbol.ToString())
            {
                case "double":
                    expr = TryGetDouble(variable);
                    return true;
                case "string":
                    expr = TryGetString(variable);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonDocument":
                    expr = TryParseDocument(variable);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonArray":
                    expr = TryParseDocument(variable);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonObjectId":
                    expr = TryGetObjectId(variable);
                    return true;
                case "bool":
                    expr = TryGetBoolean(variable);
                    return true;
                case "int":
                    expr = TryGetInt32(variable);
                    return true;
                case "long":
                    expr = TryGetInt64(variable);
                    return true;
                case "System.Guid":
                    expr = TryGetGuidWithBsonType(bsonType, variable);
                    return true;
                case "System.DateTimeOffset":
                    expr = TryGetDateTimeWithBsonType(bsonType, variable);
                    return true;
                default:
                    return false;
            }
        }
    }
}