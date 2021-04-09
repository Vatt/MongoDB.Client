﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator
{
    internal static partial class SerializerGenerator
    {
        public static readonly CollectionReadContext DictionaryReadContext = new CollectionReadContext(
            Identifier("dictionaryDocLength"),
            Identifier("dictionaryUnreaded"),
            Identifier("dictionaryEndMarker"),
            Identifier("dictionaryBsonType"),
            Identifier("dictionaryBsonName"),
            Identifier("dictionary"),
            Identifier("temp"),
            Identifier("internalDictionary"),
            new[]{ Argument(Identifier("dictionaryBsonName")), Argument(Identifier("temp")) });

        public static void ExtractDictionaryTypeArgs(INamedTypeSymbol type, out ITypeSymbol keyTypeArg, out ITypeSymbol valueTypeArg, out bool isICollectionOfValueTuple)
        {
            keyTypeArg = default;
            valueTypeArg = default;
            isICollectionOfValueTuple = false;
            if (IsCollectionOfValueTupleOrKeyValuePair(type))
            {
                var tupleOrPair = type!.TypeArguments[0] as INamedTypeSymbol;
                if (tupleOrPair!.IsTupleType && tupleOrPair!.TupleElements.Length != 2)
                {
                    ReportDictionaryKeyTypeError(type);
                }
                else if (tupleOrPair.IsTupleType && tupleOrPair!.TupleElements.Length == 2)
                {
                    isICollectionOfValueTuple = true;
                    keyTypeArg = tupleOrPair.TupleElements[0].Type;
                    valueTypeArg = tupleOrPair.TupleElements[1].Type;
                }else if (tupleOrPair.OriginalDefinition.Equals(System_Collections_Generic_KeyValuePair, SymbolEqualityComparer.Default))
                {
                    var pair = type.TypeArguments[0] as INamedTypeSymbol;
                    keyTypeArg = pair.TypeArguments[0];
                    valueTypeArg = pair.TypeArguments[1];
                }
                else
                {
                    ReportDictionaryKeyTypeError(type);
                }
                
            }
            else
            {
                keyTypeArg =  type!.TypeArguments[0];
                valueTypeArg = type.TypeArguments[1];
            }
        }
        private static MethodDeclarationSyntax TryParseDictionaryMethod(MemberContext ctx, ITypeSymbol type)
        {
            ITypeSymbol keyArg = default;
            ITypeSymbol typeArg = default;
            var named = (type as INamedTypeSymbol);
            StatementSyntax assigmentExpr = default;
            ExtractDictionaryTypeArgs(named, out keyArg, out typeArg, out var isICollectionOfValueTuple);

            if (keyArg!.Equals(System_String, SymbolEqualityComparer.Default) == false)
            {
                ReportDictionaryKeyTypeError(type);
            }

            if (isICollectionOfValueTuple)
            {
                assigmentExpr = SimpleAssignExprStatement(DictionaryReadContext.OutMessageToken, Cast(type, DictionaryReadContext.TempCollectionToken));
            }
            else
            {
                assigmentExpr = SimpleAssignExprStatement(DictionaryReadContext.OutMessageToken, DictionaryReadContext.TempCollectionToken);
            }
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            if (TryGenerateCollectionSimpleRead(ctx, trueTypeArg, DictionaryReadContext, builder))
            {
                goto RETURN;
            }
            if (TryGenerateCollectionTryParseBson(trueTypeArg, ctx.NameSym, DictionaryReadContext, builder))
            {
                goto RETURN;
            }
            if (TryGetEnumReadOperation(DictionaryReadContext.TempCollectionReadTargetToken, ctx.NameSym, trueTypeArg, true, out var enumOp))
            {
                builder.IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(DictionaryReadContext.TempCollectionToken, CollectionAddToken, 
                                                                               Argument(DictionaryReadContext.BsonNameToken),  Argument(enumOp.TempExpr))));
                goto RETURN;
            }
            ReportUnsuporterTypeError(ctx.NameSym, trueTypeArg);
        RETURN:
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: CollectionTryParseMethodName(type),
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 OutParameter(IdentifierName(type.ToString()), DictionaryReadContext.OutMessageToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                   Block(
                       SimpleAssignExprStatement(DictionaryReadContext.OutMessageToken, DefaultLiteralExpr()),
                       VarLocalDeclarationStatement(DictionaryReadContext.TempCollectionToken, ObjectCreation(ConstructCollectionType(type))),
                       IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(DictionaryReadContext.DocLenToken))),
                       VarLocalDeclarationStatement(DictionaryReadContext.UnreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                       SF.WhileStatement(
                           condition:
                               BinaryExprLessThan(
                                   BinaryExprMinus(DictionaryReadContext.UnreadedToken, ReaderRemainingExpr),
                                   BinaryExprMinus(DictionaryReadContext.DocLenToken, NumericLiteralExpr(1))),
                           statement:
                               Block(
                                   IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(DictionaryReadContext.BsonTypeToken))),
                                   IfNotReturnFalse(TryGetCString(VarVariableDeclarationExpr(DictionaryReadContext.BsonNameToken))),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(DictionaryReadContext.BsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(DictionaryReadContext.TempCollectionToken, CollectionAddToken, 
                                                                   Argument(DictionaryReadContext.BsonNameToken), Argument(DefaultLiteralExpr())),
                                           ContinueStatement
                                           )),
                                   builder.ToArray()
                                   )),
                       IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(DictionaryReadContext.EndMarkerToken))),
                       IfStatement(
                           BinaryExprNotEquals(DictionaryReadContext.EndMarkerToken, NumericLiteralExpr((byte)'\x00')),
                           Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(DictionaryReadContext.EndMarkerToken))))),
                       //SimpleAssignExprStatement(DictionaryReadContext.OutMessageToken, DictionaryReadContext.TempCollectionToken),
                       assigmentExpr,
                       ReturnTrueStatement
                       ));
        }

        private static MethodDeclarationSyntax WriteDictionaryMethod(MemberContext ctx, ITypeSymbol type)
        {
            var checkpoint = Identifier("checkpoint");
            var reserved = Identifier("reserved");
            var docLength = Identifier("docLength");
            var collection = Identifier("collection");
            var keyToken = Identifier("key");
            var valueToken = Identifier("value");
            ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            ExtractDictionaryTypeArgs(trueType as INamedTypeSymbol, out _, out var typeArg, out _);
            var writeOperation = ImmutableList.CreateBuilder<StatementSyntax>();
            
            if (typeArg.IsReferenceType)
            {
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(valueToken, NullLiteralExpr()),
                            statement: Block(WriteBsonNull(keyToken)),
                            @else: Block(WriteOperation(ctx, keyToken, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(valueToken))));
            }
            else if (typeArg.NullableAnnotation == NullableAnnotation.Annotated && typeArg.IsValueType)
            {
                var operation = WriteOperation(ctx, keyToken, ctx.NameSym, typeArg, BsonWriterToken, SimpleMemberAccess(valueToken, NullableValueToken));
                writeOperation.IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(valueToken, NullableHasValueToken), FalseLiteralExpr),
                            statement: Block(WriteBsonNull(keyToken)),
                            @else: Block(operation));
            }
            else
            {
                writeOperation.AddRange(WriteOperation(ctx, keyToken, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(valueToken)));
            }
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: VoidPredefinedType(),
                    identifier: CollectionWriteMethodName(trueType),
                    parameterList: ParameterList(
                        RefParameter(BsonWriterType, BsonWriterToken),
                        Parameter(TypeFullName(trueType), collection)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                Block(
                    VarLocalDeclarationStatement(checkpoint, WriterWrittenExpr),
                    VarLocalDeclarationStatement(reserved, WriterReserve(4)),
                    ForEachVariableStatement(
                        variable: VarValueTupleDeclarationExpr(keyToken, valueToken),
                        expression: IdentifierName(collection),
                        body: Block(writeOperation)),
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWrittenExpr, IdentifierName(checkpoint))),
                    Statement(ReservedWrite(reserved, docLength)),
                    Statement(WriterCommitExpr)
                ));
        }
    }

}
