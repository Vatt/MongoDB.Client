using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        public static void ExtractDictionaryTypeArgs(INamedTypeSymbol type, out ITypeSymbol keyTypeArg, out ITypeSymbol valueTypeArg)
        {
            keyTypeArg = default;
            valueTypeArg = default;

            if (IsCollectionOfKeyValuePair(type))
            {
                var kvPair = type!.TypeArguments[0] as INamedTypeSymbol;

                if (kvPair.OriginalDefinition.Equals(System_Collections_Generic_KeyValuePair, SymbolEqualityComparer.Default))
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
                keyTypeArg = type!.TypeArguments[0];
                valueTypeArg = type.TypeArguments[1];
            }
        }
        private static MethodDeclarationSyntax TryParseDictionaryMethod(MemberContext ctx, ITypeSymbol type)
        {
            ITypeSymbol keyArg = default;
            ITypeSymbol typeArg = default;
            var named = (type as INamedTypeSymbol);

            ExtractDictionaryTypeArgs(named, out keyArg, out typeArg);

            if (keyArg!.Equals(System_String, SymbolEqualityComparer.Default) == false)
            {
                ReportDictionaryKeyTypeError(type);
            }

            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);
            var statements = new List<StatementSyntax>();

            if (TryGenerateCollectionSimpleRead(ctx, trueTypeArg, InternalDictionaryToken, TempToken, DictionaryBsonTypeToken, statements, Argument(DictionaryBsonNameToken), Argument(TempToken)))
            {
                goto RETURN;
            }

            if (TryGenerateCollectionTryParseBson(trueTypeArg, ctx.NameSym, InternalDictionaryToken, TempToken, statements, Argument(DictionaryBsonNameToken), Argument(TempToken)))
            {
                goto RETURN;
            }

            if (TryGetEnumReadOperation(TempToken, ctx.NameSym, typeArg, true, out var enumOp))
            {
                statements.Add(IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(InternalDictionaryToken, CollectionAddToken, Argument(DictionaryBsonNameToken), Argument(enumOp.TempExpr)))));

                goto RETURN;
            }

            ReportUnsupportedTypeError(ctx.NameSym, trueTypeArg);
        RETURN:
            return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: SyntaxTokenList(PrivateKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,
                    returnType: BoolPredefinedType(),
                    identifier: CollectionTryParseMethodName(type),
                    parameterList: ParameterList(RefParameter(BsonReaderType, BsonReaderToken),
                                                 OutParameter(IdentifierName(type.ToString()), DictionaryToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                   Block(
                       SimpleAssignExprStatement(DictionaryToken, DefaultLiteralExpr()),
                       VarLocalDeclarationStatement(InternalDictionaryToken, ObjectCreation(ConstructCollectionType(type))),
                       IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(DictionaryDocLenToken))),
                       VarLocalDeclarationStatement(DictionaryUnreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                       SF.WhileStatement(
                           condition:
                               BinaryExprLessThan(
                                   BinaryExprMinus(DictionaryUnreadedToken, ReaderRemainingExpr),
                                   BinaryExprMinus(DictionaryDocLenToken, NumericLiteralExpr(1))),
                           statement:
                               Block(
                                   IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(DictionaryBsonTypeToken))),
                                   IfNotReturnFalse(TryGetCString(VarVariableDeclarationExpr(DictionaryBsonNameToken))),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(DictionaryBsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(InternalDictionaryToken,
                                                                   CollectionAddToken,
                                                                   Argument(DictionaryBsonNameToken), Argument(DefaultLiteralExpr())),
                                           ContinueStatement
                                           )),
                                   statements.ToArray()
                                   )),
                       IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(DictionaryEndMarkerToken))),
                       IfStatement(
                           BinaryExprNotEquals(DictionaryEndMarkerToken, NumericLiteralExpr((byte)'\x00')),
                           Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(DictionaryEndMarkerToken))))),
                       SimpleAssignExprStatement(DictionaryToken, InternalDictionaryToken),
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

            ExtractDictionaryTypeArgs(trueType as INamedTypeSymbol, out _, out var typeArg);

            var writeOperations = new List<StatementSyntax>();

            if (typeArg.IsReferenceType)
            {
                writeOperations.Add(
                    IfStatement(
                            condition: BinaryExprEqualsEquals(valueToken, NullLiteralExpr()),
                            statement: Block(WriteBsonNull(keyToken)),
                            @else: Block(WriteOperation(ctx, keyToken, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(valueToken)))));
            }
            else if (typeArg.NullableAnnotation == NullableAnnotation.Annotated && typeArg.IsValueType)
            {
                var operation = WriteOperation(ctx, keyToken, ctx.NameSym, typeArg, BsonWriterToken, SimpleMemberAccess(valueToken, NullableValueToken));

                writeOperations.Add(
                    IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(valueToken, NullableHasValueToken), FalseLiteralExpr),
                            statement: Block(WriteBsonNull(keyToken)),
                            @else: Block(operation)));
            }
            else
            {
                writeOperations.AddRange(WriteOperation(ctx, keyToken, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(valueToken)));
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
                        body: Block(writeOperations)),
                    WriteByteStatement((byte)'\x00'),
                    VarLocalDeclarationStatement(docLength, BinaryExprMinus(WriterWrittenExpr, IdentifierName(checkpoint))),
                    Statement(ReservedWrite(reserved, docLength)),
                    Statement(WriterCommitExpr)
                ));
        }
    }
}
