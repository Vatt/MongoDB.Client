using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var tempArrayRead = SF.Identifier("temp");

            var typeArg = (type as INamedTypeSymbol).TypeArguments[0];
            
            var parameters = ParameterList(RefParameter(ctx.Root.BsonReaderType, ctx.Root.BsonReaderToken ),
                                           OutParameter(IdentifierName(type.ToString()), outMessage));            
            var whileCondition = 
                BinaryExprLessThan(
                    BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemaining()),
                    BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1)));
            var operation = ReadOperation(ctx.Root, ctx.NameSym, typeArg, ctx.Root.BsonReaderId, tempArrayRead, bsonTypeToken, bsonNameToken);
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
                                    IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonTypeToken), NumericLiteralExpr(10))),
                                    IfNotReturnFalse(TrySkipCString()),
                                    IfNotReturnFalseElse(
                                        condition: operation, 
                                        @else:SF.Block(
                                            SF.ExpressionStatement(InvocationExpr(IdentifierName(outMessage), IdentifierName("Add"), Argument(tempArrayRead))),
                                            SF.ContinueStatement())))),
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
                var operation = ReadOperation(ctx, member.NameSym, member.TypeSym, ctx.BsonReaderId, member.AssignedVariable, bsonType, bsonName);
                statements.Add(
                    SF.IfStatement(
                        condition: SpanSequenceEqual(IdentifierName(bsonName), IdentifierName(StaticFieldNameToken(member))),
                        statement:SF.Block(
                                IfContinue(BinaryExprEqualsEquals(IdentifierName(bsonType), NumericLiteralExpr(10))),
                                IfNotReturnFalse(operation), 
                                SF.ContinueStatement())));
            }
            return statements.ToArray();
        }

        private static ExpressionSyntax ReadOperation(ClassContext ctx, ISymbol nameSym, ITypeSymbol typeSym, ExpressionSyntax readerId, 
                                                     SyntaxToken readTarget, SyntaxToken bsonType, SyntaxToken bsonName)
        {
            if (TryGetSimpleReadOperation(typeSym, IdentifierName(bsonType), IdentifierName(readTarget), out var simpleOperation))
            {
                return simpleOperation;
            }

            if ( ctx.GenericArgs?.FirstOrDefault( sym => sym.Name.Equals(typeSym.Name)) != default )
            {
                //TODO: generic read
            }
            if (typeSym is INamedTypeSymbol namedType && namedType.TypeParameters.Length > 0)
            {
                if (namedType.ToString().Contains("System.Collections.Generic.List") ||
                    namedType.ToString().Contains("System.Collections.Generic.IList"))
                {
                    return InvocationExpr(IdentifierName(ReadArrayMethodName(nameSym, typeSym)), RefArgument(readerId), OutArgument(IdentifierName(readTarget)));
                }
            }
            else
            {
                foreach (var context in ctx.Root.Contexts)
                {
                    //TODO: если сериализатор не из ЭТОЙ сборки, добавить ветку с мапой с проверкой на нуль
                    if (context.Declaration.ToString().Equals(typeSym.ToString()))
                    {
                        return GeneratedSerializerTryParse(context, readerId, readerId);
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