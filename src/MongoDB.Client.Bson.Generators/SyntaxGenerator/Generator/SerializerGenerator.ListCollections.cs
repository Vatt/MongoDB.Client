﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Diagnostics;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        private static SyntaxToken WriteListCollectionMethodName(MemberContext ctx, ITypeSymbol typeSymbol)
        {
            var name = $"Write{ctx.NameSym.Name}{typeSymbol.Name}";
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
            return Identifier(name);
        }
        private static SyntaxToken ReadListCollectionMethodName(ISymbol nameSym, ITypeSymbol typeSymbol)
        {
            var name = $"TryParse{nameSym.Name}{typeSymbol.Name}";
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
            return Identifier(name);
        }
        private static MethodDeclarationSyntax[] GenerateReadListCollectionMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members)
            {
                if (IsListCollection(member.TypeSym))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(ReadArrayMethod(member, member.TypeSym));
                        break;
                    }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(ReadArrayMethod(member, type));
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                        //if (type is null || type.TypeArguments.IsEmpty)
                        if (type is null || (IsListCollection(type) == false))
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static MethodDeclarationSyntax[] GenerateReadDictionaryMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();

            foreach (var member in ctx.Members)
            {
                if (IsDictionaryCollection(member.TypeSym))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(ReadDictionaryMethod(member, member.TypeSym));
                        break;
                    }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(ReadDictionaryMethod(member, type));
                        type = type.TypeArguments[1] as INamedTypeSymbol;
                        //if (type is null || type.TypeArguments.IsEmpty)
                        if (type is null || (IsDictionaryCollection(type) == false))
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static MethodDeclarationSyntax[] GenerateWriteListCollectionMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            foreach (var member in ctx.Members)
            {
                if (IsListCollection(member.TypeSym))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(WriteListCollectionMethod(member, member.TypeSym));
                        break;
                    }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(WriteListCollectionMethod(member, type));
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                        if (type is null || (IsListCollection(type) == false))
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static bool TryGenerateSimpleReadOpereation(MemberContext ctx, ITypeSymbol type, SyntaxToken readTarget, SyntaxToken bsonType, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            var (operation, tempVar) = ReadOperation(ctx.Root, ctx.NameSym, type, BsonReaderToken, TypedVariableDeclarationExpr(TypeFullName(type), readTarget), bsonType);
            if (operation == default)
            {
                return false;
            }
            builder.IfNotReturnFalseElse(condition: operation,
                                         @else:
                                            Block(
                                                InvocationExprStatement(outMessage, CollectionAddToken, Argument(tempVar != null ? tempVar : IdentifierName(readTarget))),
                                                ContinueStatement));
            return true;
        }
        private static bool TryGenerateTryParseBsonArrayOperation(ITypeSymbol typeSym, ISymbol nameSym, SyntaxToken readTarget, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            ITypeSymbol callType = default;
            ITypeSymbol outArgType = default;
            if (IsBsonSerializable(typeSym))
            {
                callType = typeSym;
                outArgType = typeSym;
            }

            if (IsBsonExtensionSerializable(nameSym, typeSym, out var extSym))
            {
                callType = extSym;
                outArgType = typeSym;
            }

            if (callType is null || outArgType is null)
            {
                return false;
            }
            var operation = InvocationExpr(IdentifierName(callType.ToString()), TryParseBsonToken, RefArgument(BsonReaderToken), OutArgument(TypedVariableDeclarationExpr(TypeFullName(outArgType), readTarget)));
            builder.IfNotReturnFalseElse(condition: operation,
                                         @else: Block(InvocationExprStatement(outMessage, CollectionAddToken, Argument(readTarget)), ContinueStatement));
            return true;
        }
        private static MethodDeclarationSyntax ReadArrayMethod(MemberContext ctx, ITypeSymbol type)
        {
            var docLenToken = Identifier("arrayDocLength");
            var unreadedToken = Identifier("arrayUnreaded");
            var endMarkerToken = Identifier("arrayEndMarker");
            var bsonTypeToken = Identifier("arrayBsonType");
            var bsonNameToken = Identifier("arrayBsonName");
            var outMessage = Identifier("array");
            var tempArrayRead = Identifier("temp");
            var tempArray = Identifier("internalArray");

            var typeArg = (type as INamedTypeSymbol).TypeArguments[0];
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);

            //ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            if (TryGenerateSimpleReadOpereation(ctx, trueTypeArg, tempArrayRead, bsonTypeToken, tempArray, builder))
            {
                goto RETURN;
            }
            if (TryGenerateTryParseBsonArrayOperation(trueTypeArg, ctx.NameSym, tempArrayRead, tempArray, builder))
            {
                goto RETURN;
            }
            if (TryGetEnumReadOperation(tempArrayRead, ctx.NameSym, trueTypeArg, true, out var enumOp))
            {
                builder.IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(tempArray, CollectionAddToken, Argument(enumOp.TempExpr))));
                goto RETURN;
            }
            GeneratorDiagnostics.ReportUnsuporterTypeError(ctx.NameSym, trueTypeArg);
        RETURN:
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: ReadListCollectionMethodName(ctx.NameSym, type),
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 OutParameter(IdentifierName(type.ToString()), outMessage)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                   Block(
                       SimpleAssignExprStatement(outMessage, DefaultLiteralExpr()),
                       VarLocalDeclarationStatement(tempArray, ObjectCreation(TypeFullName(System_Collections_Generic_List_T.Construct(typeArg)))),
                       IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(docLenToken))),
                       VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                       SF.WhileStatement(
                           condition:
                               BinaryExprLessThan(
                                   BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemainingExpr),
                                   BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                           statement:
                               Block(
                                   IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(bsonTypeToken))),
                                   IfNotReturnFalse(TrySkipCStringExpr),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(bsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(tempArray, CollectionAddToken, Argument(DefaultLiteralExpr())),
                                           ContinueStatement
                                           )),
                                   builder.ToArray()
                                   )),
                       IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                       IfStatement(
                           BinaryExprNotEquals(endMarkerToken, NumericLiteralExpr((byte)'\x00')),
                           Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(endMarkerToken))))),
                       SimpleAssignExprStatement(IdentifierName(outMessage), tempArray),
                       ReturnTrueStatement
                       ));
        }

        private static MethodDeclarationSyntax WriteListCollectionMethod(MemberContext ctx, ITypeSymbol type)
        {
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var sizeSpan = Identifier("sizeSpan");
            var index = Identifier("index");
            var array = Identifier("array");
            var loopItem = Identifier("item");
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[0];
            var haveCollectionIndexator = HaveCollectionIndexator(type);
            var writeOperation = ImmutableList.CreateBuilder<StatementSyntax>();
            var elementExpr = haveCollectionIndexator
                ? ElementAccessExpr(array, index)
                : IdentifierName(loopItem);

            if (typeArg.IsReferenceType)
            {
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(loopItem, NullLiteralExpr()),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(loopItem))));
            }
            else if (typeArg.NullableAnnotation == NullableAnnotation.Annotated && typeArg.IsValueType)
            {
                var operation = WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, SimpleMemberAccess(loopItem, NullableValueToken));
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(loopItem, NullableHasValueToken), FalseLiteralExpr),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(operation));
            }
            else
            {
                writeOperation.AddRange(WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(loopItem)));
            }

            var loopStatement = haveCollectionIndexator
                ? ForStatement(
                    condition: BinaryExprLessThan(index, SimpleMemberAccess(array, ListCountToken)),
                    incrementor: PostfixUnaryExpr(index),
                    body: Block(VarLocalDeclarationStatement(loopItem, elementExpr), writeOperation!))
                : ForEachStatement(
                    identifier: loopItem,
                    expression: IdentifierName(array),
                    body: Block(writeOperation!, AddAssignmentExpr(index, NumericLiteralExpr(1))));

            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: VoidPredefinedType(),
                    identifier: WriteListCollectionMethodName(ctx, trueType),
                    parameterList: ParameterList(
                        RefParameter(BsonWriterType, BsonWriterToken),
                        Parameter(TypeFullName(trueType), array)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                Block(
                    LocalDeclarationStatement(IntPredefinedType(), index, NumericLiteralExpr(0)),
                    VarLocalDeclarationStatement(checkpoint, WriterWrittenExpr),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)),
                    loopStatement,
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWrittenExpr, IdentifierName(checkpoint))),
                    Statement(ReservedWrite(reserved, docLength)),
                    Statement(WriterCommitExpr)
                ));
        }
    }

}
