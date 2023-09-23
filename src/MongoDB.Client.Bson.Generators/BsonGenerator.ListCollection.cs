using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators
{
    public partial class BsonGenerator
    {
        private static MethodDeclarationSyntax TryParseListCollectionMethod(MemberContext ctx, ITypeSymbol type)
        {
            var typeArg = (type as INamedTypeSymbol).TypeArguments[0];
            var trueTypeArg = ExtractTypeFromNullableIfNeed(typeArg);

            var statements = new List<StatementSyntax>();

            if (TryGenerateCollectionTryParseBson(trueTypeArg, ctx.NameSym, InternalListToken, TempToken, statements, Argument(TempToken)))
            {
                goto RETURN;
            }

            if (TryGenerateCollectionSimpleRead(ctx, trueTypeArg, InternalListToken, TempToken, ListBsonTypeToken, statements, Argument(TempToken)))
            {
                goto RETURN;
            }

            if (TryGetEnumReadOperation(TempToken, ctx.NameSym, typeArg, true, out var enumOp))
            {
                statements.Add(IfNotReturnFalseElse(enumOp.Expr, Block(InvocationExpr(InternalListToken, CollectionAddToken, Argument(enumOp.TempExpr)))));
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
                                                 OutParameter(IdentifierName(type.ToString()), ListToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(
                   Block(
                       SimpleAssignExprStatement(ListToken, DefaultLiteralExpr()),
                       VarLocalDeclarationStatement(InternalListToken, ObjectCreation(ConstructCollectionType(type))),
                       IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(ListDocLenToken))),
                       VarLocalDeclarationStatement(ListUnreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                       SF.WhileStatement(
                           condition:
                               BinaryExprLessThan(
                                   BinaryExprMinus(ListUnreadedToken, ReaderRemainingExpr),
                                   BinaryExprMinus(ListDocLenToken, NumericLiteralExpr(1))),
                           statement:
                               Block(
                                   IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(ListBsonTypeToken))),
                                   IfNotReturnFalse(TrySkipCStringExpr),
                                   IfStatement(
                                       condition: BinaryExprEqualsEquals(ListBsonTypeToken, NumericLiteralExpr(10)),
                                       statement: Block(
                                           InvocationExprStatement(InternalListToken, CollectionAddToken, Argument(DefaultLiteralExpr())),
                                           ContinueStatement
                                           )),
                                   statements.ToArray()
                                   )),
                       IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(ListEndMarkerToken))),
                       IfStatement(
                               BinaryExprNotEquals(ListEndMarkerToken, NumericLiteralExpr((byte)'\x00')),
                               Block(Statement(SerializerEndMarkerException(ctx.Root.Declaration, IdentifierName(ListEndMarkerToken))))),
                       SimpleAssignExprStatement(ListToken, InternalListToken),
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
            var haveCollectionIndexator = HaveCollectionIntIndexator(type);
            var writeOperation = ImmutableList.CreateBuilder<StatementSyntax>();
            var elementExpr = haveCollectionIndexator
                ? ElementAccessExpr(array, index)
                : IdentifierName(loopItem);

            if (typeArg.IsReferenceType)
            {
                writeOperation.Add(
                    IfStatement(
                            condition: BinaryExprEqualsEquals(loopItem, NullLiteralExpr()),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, IdentifierName(loopItem)))));
            }
            else if (typeArg.NullableAnnotation == NullableAnnotation.Annotated && typeArg.IsValueType)
            {
                var operation = WriteOperation(ctx, index, ctx.NameSym, typeArg, BsonWriterToken, SimpleMemberAccess(loopItem, NullableValueToken));

                writeOperation.Add(
                    IfStatement(
                            condition: BinaryExprEqualsEquals(SimpleMemberAccess(loopItem, NullableHasValueToken), FalseLiteralExpr),
                            statement: Block(WriteBsonNull(index)),
                            @else: Block(operation)));
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
                    identifier: CollectionWriteMethodName(trueType),
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
