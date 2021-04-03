using System.Collections.Generic;
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
        private static bool TryGenerateSimpleReadOpereation1(MemberContext ctx, ITypeSymbol type, SyntaxToken readTarget, SyntaxToken bsonName, SyntaxToken bsonType, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
        {
            var (operation, tempVar) = ReadOperation(ctx.Root, ctx.NameSym, type, BsonReaderToken, TypedVariableDeclarationExpr(TypeFullName(type), readTarget), bsonType);
            if (operation == default)
            {
                return false;
            }
            builder.IfNotReturnFalseElse(condition: operation,
                                         @else:
                                            Block(
                                                InvocationExprStatement(outMessage, CollectionAddToken, Argument(bsonName),  Argument(tempVar != null ? tempVar : IdentifierName(readTarget))),
                                                ContinueStatement));
            return true;
        }
        private static bool TryGenerateTryParseBsonArrayOperation1(ITypeSymbol typeSym, ISymbol nameSym, SyntaxToken bsonName, SyntaxToken readTarget, SyntaxToken outMessage, ImmutableList<StatementSyntax>.Builder builder)
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
                                         @else: Block(InvocationExprStatement(outMessage, CollectionAddToken, Argument(bsonName), Argument(readTarget)), ContinueStatement));
            return true;
        }
        private static MethodDeclarationSyntax TryParseDictionaryMethod(MemberContext ctx, ITypeSymbol type)
        {
            var docLenToken = Identifier("collectionDocLength");
            var unreadedToken = Identifier("collectionUnreaded");
            var endMarkerToken = Identifier("collectionEndMarker");
            var bsonTypeToken = Identifier("collectionBsonType");
            var bsonNameToken = Identifier("collectionBsonName");
            var outMessage = Identifier("collection");
            var tempArrayRead = Identifier("temp");
            var tempArray = Identifier("internalCollection");
            var typeArg = (type as INamedTypeSymbol).TypeArguments[1];
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);

            //ITypeSymbol trueType = ExtractTypeFromNullableIfNeed(type);
            var builder = ImmutableList.CreateBuilder<StatementSyntax>();
            if (TryGenerateSimpleReadOpereation1(ctx, trueTypeArg, tempArrayRead, bsonNameToken, bsonTypeToken, tempArray, builder))
            {
                goto RETURN;
            }
            if (TryGenerateTryParseBsonArrayOperation1(trueTypeArg, ctx.NameSym, bsonNameToken, tempArrayRead, tempArray, builder))
            {
                goto RETURN;
            }
            if (TryGetEnumReadOperation(tempArrayRead, ctx.NameSym, trueTypeArg, true, out var enumOp))
            {
                builder.IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(tempArray, CollectionAddToken, Argument(bsonNameToken),  Argument(enumOp.TempExpr))));
                goto RETURN;
            }
            GeneratorDiagnostics.ReportUnsuporterTypeError(ctx.NameSym, trueTypeArg);
        RETURN:
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: CollectionTryParseMethodName(type),
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
                       //VarLocalDeclarationStatement(tempArray, ObjectCreation(TypeFullName(System_Collections_Generic_Dictionary_K_V.Construct(System_String, typeArg)))),
                       VarLocalDeclarationStatement(tempArray, ObjectCreation(ConstructCollectionType(type))),
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
            var collection = Identifier("collection");
            var keyToken = Identifier("key");
            var valueToken = Identifier("value");
            var typeArg = (trueType as INamedTypeSymbol).TypeArguments[1];

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
