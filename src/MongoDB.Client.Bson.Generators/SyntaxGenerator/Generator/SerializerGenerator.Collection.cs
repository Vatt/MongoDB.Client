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
        public class CollectionWroteContext
        {
            public ISymbol Declaration;
            public ExpressionSyntax BsonNameReadOperationExpr;
            public ObjectCreationExpressionSyntax LocalCollectionCreateExpr;
            public ImmutableList<StatementSyntax>.Builder BsonValueReadOperation;
            public SyntaxToken BsonTypeToken;
            public SyntaxToken OutMessageToken;
            public SyntaxToken TempCollection;
        }
        public class CollectionWriteContext
        {
            public ISymbol Declaration;
            public ExpressionSyntax BsonNameWriteOperationExpr;
            public ImmutableList<StatementSyntax>.Builder BsonValueWriteOperation;
        }
        public static BlockSyntax CollectionTryParseBody(CollectionWroteContext ctx)
        {
            var docLenToken = Identifier("collectionDocLength");
            var unreadedToken = Identifier("collectionUnreaded");
            var endMarkerToken = Identifier("collectionEndMarker");

            return Block(
                SimpleAssignExprStatement(ctx.OutMessageToken, DefaultLiteralExpr()),
                VarLocalDeclarationStatement(ctx.TempCollection, ctx.LocalCollectionCreateExpr),
                IfNotReturnFalse(TryGetInt32(IntVariableDeclarationExpr(docLenToken))),
                VarLocalDeclarationStatement(unreadedToken, BinaryExprPlus(ReaderRemainingExpr, SizeOfInt32Expr)),
                SF.WhileStatement(
                    condition:
                    BinaryExprLessThan(
                        BinaryExprMinus(IdentifierName(unreadedToken), ReaderRemainingExpr),
                        BinaryExprMinus(IdentifierName(docLenToken), NumericLiteralExpr(1))),
                    statement:
                    Block(
                        IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(ctx.BsonTypeToken))),
                        IfNotReturnFalse(ctx.BsonNameReadOperationExpr),
                        IfStatement(
                            condition: BinaryExprEqualsEquals(ctx.BsonTypeToken, NumericLiteralExpr(10)),
                            statement: Block(
                                InvocationExprStatement(ctx.TempCollection, ListAddToken, Argument(DefaultLiteralExpr())),
                                ContinueStatement
                            )),
                        ctx.BsonValueReadOperation
                    )),
                IfNotReturnFalse(TryGetByte(VarVariableDeclarationExpr(endMarkerToken))),
                IfStatement(
                    BinaryExprNotEquals(endMarkerToken, NumericLiteralExpr((byte)'\x00')),
                    Block(Statement(SerializerEndMarkerException(ctx.Declaration, IdentifierName(endMarkerToken))))),
                SimpleAssignExprStatement(IdentifierName(ctx.OutMessageToken), ctx.TempCollection),
                ReturnTrueStatement
            );
        }

        public static BlockSyntax CollectionWriteBody(CollectionWroteContext ctx)
        {
            return default;
        }
    }
}
