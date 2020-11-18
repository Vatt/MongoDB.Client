using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken ReadArrayMethodName(ISymbol nameSym, ITypeSymbol typeSymbol)
        {
            var name =  $"TryParse{nameSym.Name}{typeSymbol.Name}";/*{typeSymbol.GetTypeMembers()[0].Name}*/
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
            List<MethodDeclarationSyntax> methods = new ();
            
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
                    if (type.TypeArguments[0] is INamedTypeSymbol arg1 && arg1.TypeArguments.Length == 0)
                    {
                        methods.Add(ReadArrayMethod(member, type));
                    }
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

            var typeArg = (type as INamedTypeSymbol).TypeArguments[0];
            
            var parameters = ParameterList(RefParameter(ctx.Root.BsonReaderType, ctx.Root.BsonReaderToken ),
                                           OutParameter(IdentifierName(type.ToString()), outMessage));            
            var whileCondition = 
                BinaryExprLessThan(
                    BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                    BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1)));

            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword()), 
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: ReadArrayMethodName(ctx.NameSym, type),
                    parameterList: parameters,
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                    SF.Block(
                        SF.ExpressionStatement(SimpleAssignExpr(IdentifierName(outMessage), ObjectCreation(TypeFullName(type)))),
                        IfNotReturnFalse(TryGetInt32(VarVariableDeclarationExpr(docLenToken))),
                        VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemaining(), SizeOfInt())),
                        SF.WhileStatement(
                            whileCondition,
                            SF.Block(
                                    IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonTypeToken))),
                                    IfNotReturnFalse(TrySkip(IdentifierName(bsonTypeToken))))
                                .AddStatements(ReadArrayOperation(ctx.NameSym, typeArg, ctx.AssignedVariable, bsonTypeToken, bsonNameToken))
                                .AddStatements(IfNotReturnFalse(TrySkip(IdentifierName(bsonTypeToken))))
                        ),
                        IfNotReturnFalse(TryGetByte(IdentifierName(endMarkerToken))),
                        SF.IfStatement(
                            BinaryExprNotEquals(IdentifierName(endMarkerToken), NumericLiteralExpr((byte)'\x00')),
                            SF.Block(SF.ExpressionStatement(SerializerEndMarkerException(IdentifierName(SerializerName(ctx.Root)), IdentifierName(endMarkerToken))))),
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
            
            var parameters = ParameterList(RefParameter(ctx.BsonReaderType, ctx.BsonReaderToken ),
                                           OutParameter(TypeFullName(ctx.Declaration), ctx.TryParseOutVarToken));
            var whileCondition = 
                BinaryExprLessThan(
                    BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                    BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1)));
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: default,
                    explicitInterfaceSpecifier: SF.ExplicitInterfaceSpecifier(GenericName(SerializerToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: SF.Identifier("TryParse"),
                    parameterList: parameters,
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(SF.Block(
                    DeclareTempVariables(ctx)).AddStatements(
                    IfNotReturnFalse(TryGetInt32(VarVariableDeclarationExpr(docLenToken))),
                    VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemaining(), SizeOfInt())),
                    SF.WhileStatement(whileCondition,TryParseWhileStatements(ctx,bsonTypeToken, bsonNameToken)),
                    IfNotReturnFalse(TryGetByte(IdentifierName(endMarkerToken))),
                    SF.IfStatement(
                        condition: BinaryExprNotEquals(IdentifierName(endMarkerToken), NumericLiteralExpr((byte)'\x00')),
                        statement: SF.Block(SF.ExpressionStatement(SerializerEndMarkerException(IdentifierName(SerializerName(ctx)), IdentifierName(endMarkerToken))))),
                    SF.ReturnStatement(TrueLiteralExpr())));
        }

        /*private static BlockSyntax TryParseBody(ClassContext ctx)
        {
            var decl = ctx.Declaration;
            var docLenToken = SF.Identifier("docLength");
            var unreadedToken = SF.Identifier("unreaded");
            var endMarkerToken = SF.Identifier("endMarker");
            var bsonTypeToken = SF.Identifier("bsonType");
            var bsonNameToken = SF.Identifier("bsonName");
            var whileCondition = 
                BinaryExprLessThan(
                    BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                    BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1)));
            return SF.Block(DeclareTempVariables(ctx)).AddStatements(
                    IfNotReturnFalse(TryGetInt32(VarVariableDeclarationExpr(docLenToken))),
                    VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemaining(), SizeOfInt())),
                    SF.WhileStatement(whileCondition,TryParseWhileStatements(ctx,bsonTypeToken, bsonNameToken)),
                    IfNotReturnFalse(TryGetByte(IdentifierName(endMarkerToken))),
                    SF.IfStatement(
                        condition: BinaryExprNotEquals(IdentifierName(endMarkerToken), NumericLiteralExpr((byte)'\x00')),
                        statement: SF.Block(SF.ExpressionStatement(SerializerEndMarkerException(IdentifierName(SerializerName(ctx)), IdentifierName(endMarkerToken))))),
                    SF.ReturnStatement(TrueLiteralExpr()));
        }*/

        private static StatementSyntax[] DeclareTempVariables(ClassContext ctx)
        {
            List<StatementSyntax> variables = new();
            foreach (var member in ctx.Members)
            {
                member.AssignedVariable =  SF.Identifier($"{member.TypeSym.Name}{member.NameSym.Name}");
                variables.Add(DefaultLocalDeclarationStatement(SF.ParseTypeName(member.TypeSym.ToString()), member.AssignedVariable));
            }

            return variables.ToArray();
        }
        private static BlockSyntax TryParseWhileStatements(ClassContext ctx, SyntaxToken bsonType,SyntaxToken bsonName)
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
                statements.Add(
                    SF.IfStatement(
                        condition: SpanSequenceEqual(IdentifierName(bsonName), IdentifierName(StaticFieldNameToken(member))),
                        statement:SF.Block(IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonType), NumericLiteralExpr(10))))
                                    .AddStatements(ReadOperation(member, bsonType, bsonName))));
            }
            return statements.ToArray();
        }

        private static StatementSyntax[] ReadOperation(MemberContext ctx,SyntaxToken bsonType, SyntaxToken bsonName)
        {
            if (TryGetReadOperation(ctx.TypeSym, IdentifierName(bsonType), IdentifierName(ctx.AssignedVariable), out var simpleOperation))
            {
                return new StatementSyntax[]
                {
                    IfNotReturnFalse(simpleOperation),
                    SF.ContinueStatement()
                };
            }

            if (ctx.IsGenericType )
            {
                //TODO: generic read
            }
            if (ctx.TypeGenericArgs.HasValue)
            {
                if (ctx.TypeSym.ToString().Contains("System.Collections.Generic.List") ||
                    ctx.TypeSym.ToString().Contains("System.Collections.Generic.IList"))
                {
                    /*
                    var invocation =
                        InvocationExpr(IdentifierName(ReadArrayMethodName(ctx.TypeSym)),
                                       RefArgument(ctx.Root.BsonReaderId), 
                                       OutArgument(IdentifierName(ctx.AssignedVariable)));
                    return new StatementSyntax[]
                    {
                        IfNotReturnFalse(invocation), 
                        SF.ContinueStatement()
                    };*/
                    return ReadArrayOperation(ctx.NameSym, ctx.TypeSym, ctx.AssignedVariable, bsonType, bsonName);
                }
            }
            else
            {
                foreach (var context in ctx.Root.Root.Contexts)
                {
                    //TODO: если сериализатор не из ЭТОЙ сборки, добавить ветку с мапой с проверкой на нуль
                    if (context.Declaration.Name.Equals(ctx.Root.Declaration.Name))
                    {
                        return new StatementSyntax[]
                        {
                            SF.ExpressionStatement(GeneratedSerializerTryParse(context, ctx.Root.BsonReaderId, IdentifierName(ctx.AssignedVariable))),
                            SF.ContinueStatement()
                        };
                    }
                }
            }
            return new StatementSyntax[] { SF.ContinueStatement() };
        }
       
        private static StatementSyntax[] ReadArrayOperation(ISymbol nameSym, ITypeSymbol typeSym, SyntaxToken readTarget, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            if (TryGetReadOperation(typeSym, IdentifierName(bsonType), IdentifierName(readTarget), out var simpleOperation))
            {
                return new StatementSyntax[]
                {
                    IfNotReturnFalse(simpleOperation),
                    SF.ContinueStatement()
                };
            }
            /*
            if (ctx.IsGenericType )
            {
                //TODO: generic read
            }
            */
            if (typeSym.ToString().Contains("System.Collections.Generic.List") ||
                typeSym.ToString().Contains("System.Collections.Generic.IList"))
            {
                var invocation =
                    InvocationExpr(IdentifierName(ReadArrayMethodName(nameSym, typeSym)),
                        RefArgument(IdentifierName("reader")), 
                        OutArgument(IdentifierName(readTarget)));
                return new StatementSyntax[]
                {
                    IfNotReturnFalse(invocation), 
                    SF.ContinueStatement()
                };
            }
            else
            {
                /*
                foreach (var context in ctx.Root.Root.Contexts)
                {
                    //TODO: если сериализатор не из ЭТОЙ сборки, добавить ветку с мапой с проверкой на нуль
                    if (context.Declaration.Name.Equals(ctx.Root.Declaration.Name))
                    {
                        return new StatementSyntax[]
                        {
                            SF.ExpressionStatement(GeneratedSerializerTryParse(context, ctx.Root.BsonReaderId, IdentifierName(ctx.AssignedVariable))),
                            SF.ContinueStatement()
                        };
                    }
                }
                */
            }
            return new StatementSyntax[] { SF.ContinueStatement() };
        } 
        
        private static bool TryGetReadOperation(ITypeSymbol typeSymbol, ExpressionSyntax bsonType, ExpressionSyntax variable, out InvocationExpressionSyntax expr)
        {
            expr = default;
            switch (typeSymbol.ToString())
            {
                //case "System.Double":
                case "double":
                    expr = TryGetDouble(variable);
                    return true;
                //case "System.String":
                case "string":
                    expr = TryGetString(variable);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonDocument":
                    expr = TryParseDocument(variable);
                    return true;
                case "MongoDB.Client.Bson.Document.BsonObjectId":
                    expr = TryGetObjectId(variable);
                    return true;
                //case "System.Boolean":
                case "bool":
                    expr = TryGetBoolean(variable);
                    return true;
                //case "System.Int32":
                case "int":
                    expr = TryGetInt32(variable);
                    return true;
                //case "System.Int64":
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