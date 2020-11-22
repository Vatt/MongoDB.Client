using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken ReadArrayMethodName(ISymbol nameSym, ITypeSymbol typeSymbol)
        {
            var name = $"TryParse{nameSym.Name}{typeSymbol.Name}";/*{typeSymbol.GetTypeMembers()[0].Name}*/
            var type = typeSymbol as INamedTypeSymbol;

            while (true)
            {
                name = $"{name}{type.TypeArguments[0].Name}";
                type = type.TypeArguments[0] as INamedTypeSymbol;
                if (type is null || type.TypeArguments.IsEmpty)
                {
                    break;
                }
            }
            return SF.Identifier(name);
        }

        private static MethodDeclarationSyntax[] GenerateReadArrayMethods(ClassContext ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members)
            {
                if (member.TypeSym.ToString().Contains("System.Collections.Generic.List") ||
                    member.TypeSym.ToString().Contains("System.Collections.Generic.IList"))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(ReadArrayMethod(member, member.TypeSym));
                        break;
                    }
                    // if (type.TypeArguments[0] is INamedTypeSymbol arg1 && arg1.TypeArguments.Length == 0)
                    // {
                    //     methods.Add(ReadArrayMethod(member, type));
                    //     continue;
                    // }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(ReadArrayMethod(member, type));
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                        if (type is null || type.TypeArguments.IsEmpty)
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static MethodDeclarationSyntax ReadArrayMethod(MemberContext ctx, ITypeSymbol type)
        {
            var docLenToken = SF.Identifier($"arrayDocLength");
            var unreadedToken = SF.Identifier($"arrayUnreaded");
            var endMarkerToken = SF.Identifier($"arrayEndMarker");
            var bsonTypeToken = SF.Identifier($"arrayBsonType");
            var bsonNameToken = SF.Identifier($"arrayBsonName");
            var outMessage = SF.Identifier("array");
            var tempArrayRead = SF.Identifier("temp");

            var typeArg = (type as INamedTypeSymbol).TypeArguments[0];

            var operation = ReadOperation(ctx.Root, ctx.NameSym, typeArg, ctx.Root.BsonReaderId,
                TypedVariableDeclarationExpr(TypeFullName(typeArg), tempArrayRead), bsonTypeToken, bsonNameToken);
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: ReadArrayMethodName(ctx.NameSym, type),
                    parameterList: ParameterList(RefParameter(ctx.Root.BsonReaderType, ctx.Root.BsonReaderToken),
                                                 OutParameter(IdentifierName(type.ToString()), outMessage)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                    SF.Block(
                        SimpleAssignExprStatement(IdentifierName(outMessage), ObjectCreation(TypeFullName(type))),
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
                                    IfNotReturnFalse(TrySkipCString()),
                                    SF.IfStatement(
                                        condition: BinaryExprEqualsEquals(IdentifierName(bsonTypeToken), NumericLiteralExpr(10)),
                                        statement: SF.Block(
                                            InvocationExprStatement(IdentifierName(outMessage), IdentifierName("Add"), Argument(DefaultLiteralExpr())),
                                            SF.ContinueStatement()
                                            )),
                                    IfNotReturnFalseElse(
                                        condition: operation,
                                        @else:
                                            SF.Block(
                                                InvocationExprStatement(IdentifierName(outMessage), IdentifierName("Add"), Argument(tempArrayRead)),
                                                SF.ContinueStatement())))),
                        IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                        SF.IfStatement(
                            BinaryExprNotEquals(IdentifierName(endMarkerToken), NumericLiteralExpr((byte)'\x00')),
                            SF.Block(SF.ExpressionStatement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(endMarkerToken))))),
                        SF.ReturnStatement(TrueLiteralExpr())
                        ));

        }
        private static MethodDeclarationSyntax TryParseMethod(ClassContext ctx)
        {
            var decl = ctx.Declaration;
            var docLenToken = SF.Identifier("docLength");
            var unreadedToken = SF.Identifier("unreaded");
            var endMarkerToken = SF.Identifier("endMarker");
            var bsonTypeToken = SF.Identifier("bsonType");
            var bsonNameToken = SF.Identifier("bsonName");
            var body = ctx.Declaration.TypeKind == TypeKind.Enum ?
                TryParseEnumBody(ctx) :
                TryParseDefaultBody(ctx, docLenToken, unreadedToken, bsonTypeToken, bsonNameToken, endMarkerToken);
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
                .WithBody(body);
        }

        private static BlockSyntax TryParseEnumBody(ClassContext ctx)
        {
            int representation = AttributeHelper.GetEnumRepresentation(ctx.Declaration);
            return SF.Block(
                representation == 1 ? StringRepresentation(ctx) : NumericRepresentation(ctx, representation)
            );

            static StatementSyntax[] StringRepresentation(ClassContext ctx)
            {

                var stringData = SF.Identifier("stringData");
                List<StatementSyntax> statements = new()
                {
                    SimpleAssignExprStatement(ctx.TryParseOutVar, DefaultLiteralExpr()),
                    IfNotReturnFalse(TryGetStringAsSpan(VarVariableDeclarationExpr(stringData)))
                };
                foreach (var member in ctx.Members)
                {
                    statements.Add(
                        SF.IfStatement(
                            condition: SpanSequenceEqual(IdentifierName(stringData), IdentifierName(StaticFieldNameToken(member))),
                            statement: SF.Block(
                                SimpleAssignExprStatement(ctx.TryParseOutVar, IdentifierFullName(member.NameSym)),
                                SF.ReturnStatement(TrueLiteralExpr())
                                )));
                }
                statements.Add(SF.ReturnStatement(TrueLiteralExpr()));
                return statements.ToArray();
            }

            static StatementSyntax[] NumericRepresentation(ClassContext ctx, int representation)
            {
                var numericData = SF.Identifier("numericData");
                var method = representation == 2
                    ? TryGetInt32(VarVariableDeclarationExpr(numericData))
                    : TryGetInt64(VarVariableDeclarationExpr(numericData));
                return new List<StatementSyntax>()
                {
                    SimpleAssignExprStatement(ctx.TryParseOutVar, DefaultLiteralExpr()),
                    IfNotReturnFalse(method),
                    SimpleAssignExprStatement(ctx.TryParseOutVar, Cast(ctx.Declaration, IdentifierName(numericData))),
                    SF.ReturnStatement(TrueLiteralExpr())
                }.ToArray();

            }
        }
        private static BlockSyntax TryParseDefaultBody(ClassContext ctx, SyntaxToken docLenToken,
            SyntaxToken unreadedToken, SyntaxToken bsonTypeToken, SyntaxToken bsonNameToken, SyntaxToken endMarkerToken)
        {
            return SF.Block(
                    SimpleAssignExprStatement(ctx.TryParseOutVar, DefaultLiteralExpr()))
                    .AddStatements(DeclareTempVariables(ctx)).AddStatements(
                    IfNotReturnFalse(TryGetInt32(VarVariableDeclarationExpr(docLenToken))),
                    VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemaining(), SizeOfInt())),
                    SF.WhileStatement(
                        condition:
                        BinaryExprLessThan(
                            BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                            BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                        statement: TryParseWhileStatements(ctx, bsonTypeToken, bsonNameToken)),
                    IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                    SF.IfStatement(
                        condition: BinaryExprNotEquals(IdentifierName(endMarkerToken),
                            NumericLiteralExpr((byte)'\x00')),
                        statement: SF.Block(SF.ExpressionStatement(
                            SerializerEndMarkerException(ctx.Declaration, IdentifierName(endMarkerToken))))))
                .AddStatements(CreateMessage(ctx))
                .AddStatements(SF.ReturnStatement(TrueLiteralExpr()));
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
        private static BlockSyntax TryParseWhileStatements(ClassContext ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            return
                SF.Block(
                    IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonType))),
                    IfNotReturnFalse(TryGetCStringAsSpan(VarVariableDeclarationExpr(bsonName))))
                    .AddStatements(Operations(ctx, bsonType, bsonName))
                    .AddStatements(IfNotReturnFalse(TrySkip(IdentifierName(bsonType))));
        }

        private static StatementSyntax[] Operations(ClassContext ctx, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            var decl = ctx.Declaration;
            var members = ctx.Members;
            List<StatementSyntax> statements = new();

            foreach (var member in members)
            {
                var operation = ReadOperation(ctx, member.NameSym, member.TypeSym, ctx.BsonReaderId, IdentifierName(member.AssignedVariable), bsonType, bsonName);
                statements.Add(
                    SF.IfStatement(
                        condition: SpanSequenceEqual(IdentifierName(bsonName), IdentifierName(StaticFieldNameToken(member))),
                        statement: SF.Block(
                                IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonType), NumericLiteralExpr(10))),
                                IfNotReturnFalse(operation),
                                SF.ContinueStatement())));
            }
            return statements.ToArray();
        }

        private static ExpressionSyntax ReadOperation(ClassContext ctx, ISymbol nameSym, ITypeSymbol typeSym, ExpressionSyntax readerId,
                                                      ExpressionSyntax readTarget, SyntaxToken bsonType, SyntaxToken bsonName)
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