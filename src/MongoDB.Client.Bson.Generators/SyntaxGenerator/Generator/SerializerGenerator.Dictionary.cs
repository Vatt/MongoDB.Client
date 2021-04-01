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
        private static SyntaxToken WriteDictionaryMethodName(MemberContext ctx, ITypeSymbol typeSymbol)
        {
            var name = $"Write{ctx.NameSym.Name}{typeSymbol.Name}";
            var type = typeSymbol as INamedTypeSymbol;

            while (true)
            {
                name = $"{name}{type.TypeArguments[0].Name}";
                type = type.TypeArguments[1] as INamedTypeSymbol;
                if (type is null || type.TypeArguments.IsEmpty)
                {
                    break;
                }
            }
            return Identifier(name);
        }
        private static SyntaxToken ReadDictionaryMethodName(ISymbol nameSym, ITypeSymbol typeSymbol)
        {
            var name = $"TryParse{nameSym.Name}{typeSymbol.Name}";
            var type = typeSymbol as INamedTypeSymbol;

            while (true)
            {
                name = $"{name}{type.TypeArguments[0].Name}";
                type = type.TypeArguments[1] as INamedTypeSymbol;
                if (type is null || type.TypeArguments.IsEmpty)
                {
                    break;
                }
            }
            return Identifier(name);
        }
        private static MethodDeclarationSyntax[] GenerateDictionaryArrayMethods(ContextCore ctx)
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
        private static MethodDeclarationSyntax[] GenerateWriteDictionaryMethods(ContextCore ctx)
        {
            List<MethodDeclarationSyntax> methods = new();
            foreach (var member in ctx.Members)
            {
                if (IsDictionaryCollection(member.TypeSym))
                {
                    var type = member.TypeSym as INamedTypeSymbol;
                    if (type is null)
                    {
                        methods.Add(WriteDictionaryMethod(member, member.TypeSym));
                        break;
                    }
                    type = member.TypeSym as INamedTypeSymbol;
                    while (true)
                    {
                        methods.Add(WriteDictionaryMethod(member, type));
                        type = type.TypeArguments[0] as INamedTypeSymbol;
                        if (type is null || (IsDictionaryCollection(type) == false))
                        {
                            break;
                        }
                    }
                }
            }

            return methods.ToArray();
        }
        private static bool TryGenerateSimpleReadOpereation1(MemberContext ctx, ITypeSymbol type, SyntaxToken readTarget, SyntaxToken bsonType, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            var (operation, tempVar) = ReadOperation(ctx.Root, ctx.NameSym, type, BsonReaderToken, TypedVariableDeclarationExpr(TypeFullName(type), readTarget), bsonType);
            if (operation == default)
            {
                return false;
            }
            builder.IfNotReturnFalseElse(condition: operation,
                                         @else:
                                            Block(
                                                InvocationExprStatement(outMessage, CollectionAddToken, Argument(Identifier("arrayBsonName")),  Argument(tempVar != null ? tempVar : IdentifierName(readTarget))),
                                                ContinueStatement));
            return true;
        }
        private static bool TryGenerateTryParseBsonArrayOperation1(ITypeSymbol typeSym, ISymbol nameSym, SyntaxToken readTarget, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
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
                                         @else: Block(InvocationExprStatement(outMessage, CollectionAddToken, Argument(Identifier("arrayBsonName")), Argument(readTarget)), ContinueStatement));
            return true;
        }
        private static MethodDeclarationSyntax ReadDictionaryMethod(MemberContext ctx, ITypeSymbol type)
        {
            var docLenToken = Identifier("arrayDocLength");
            var unreadedToken = Identifier("arrayUnreaded");
            var endMarkerToken = Identifier("arrayEndMarker");
            var bsonTypeToken = Identifier("arrayBsonType");
            var bsonNameToken = Identifier("arrayBsonName");
            var outMessage = Identifier("array");
            var tempArrayRead = Identifier("temp");
            var tempArray = Identifier("internalArray");
            var typeArg = (type as INamedTypeSymbol).TypeArguments[1];
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);

            //ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            if (TryGenerateSimpleReadOpereation1(ctx, trueTypeArg, tempArrayRead, bsonTypeToken, tempArray, builder))
            {
                goto RETURN;
            }
            if (TryGenerateTryParseBsonArrayOperation1(trueTypeArg, ctx.NameSym, tempArrayRead, tempArray, builder))
            {
                goto RETURN;
            }
            if (TryGetEnumReadOperation(tempArrayRead, ctx.NameSym, trueTypeArg, true, out var enumOp))
            {
                builder.IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(tempArray, CollectionAddToken, Argument(Identifier("arrayBsonName")),  Argument(enumOp.TempExpr))));
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
                       VarLocalDeclarationStatement(tempArray, ObjectCreation(TypeFullName(System_Collections_Generic_Dictionary_K_V.Construct(System_String, typeArg)))),
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
                                   IfNotReturnFalse(TryGetCString(VarVariableDeclarationExpr(bsonNameToken))),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(bsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(tempArray, CollectionAddToken, Argument(bsonNameToken), Argument(DefaultLiteralExpr())),
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

        private static MethodDeclarationSyntax WriteDictionaryMethod(MemberContext ctx, ITypeSymbol type)
        {
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var sizeSpan = Identifier("sizeSpan");
            var index = Identifier("index");
            var array = Identifier("array");
            var loopItem = Identifier("item");
            //var keyExpr = SimpleMemberAccess(loopItem, Identifier("Key"));
            var keyExpr = Identifier("item.Key");
            //var valueExpr = SimpleMemberAccess(loopItem, Identifier("Value"));
            var valueExpr = Identifier("item.Value");
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[1];
            var haveCollectionIndexator = HaveCollectionIndexator(type);
            var writeOperation = ImmutableList.CreateBuilder<StatementSyntax>();
            
            if (typeArg.IsReferenceType)
            {
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(valueExpr, NullLiteralExpr()),
                            statement: Block(WriteBsonNull(keyExpr)),
                            @else: Block(WriteOperation(ctx, keyExpr, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(valueExpr))));
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
                writeOperation.AddRange(WriteOperation(ctx, keyExpr, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(valueExpr)));
            }

            var loopStatement =  ForEachStatement(
                    identifier: loopItem,
                    expression: IdentifierName(array),
                    body: Block(writeOperation));

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
