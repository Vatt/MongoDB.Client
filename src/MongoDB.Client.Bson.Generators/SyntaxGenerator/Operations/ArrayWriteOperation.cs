using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class ArrayWriteOperation : OperationBase
    {
        private static List<string> _simpleTypes = new List<string>() { "Double", "String", "BsonDocument", "BsonObjectId", "Boolean", "Int32", "Int64", "Guid", "DateTimeOffset" };
        private static IdentifierNameSyntax ForIndexId = SF.IdentifierName("index");
        private static SyntaxToken ForIndexToken = SF.Identifier("index");
        private static ExpressionSyntax reservedSizeExpr = SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(4));
        private static IdentifierNameSyntax writerWritePropId = SF.IdentifierName("Written");
        private IdentifierNameSyntax GetReserveMethodId() => SF.IdentifierName("Reserve");
        private SyntaxToken GetReservedToken() => SF.Identifier($"{MemberDecl.DeclSymbol.Name}Reserved");
        private IdentifierNameSyntax GetReservedId() => SF.IdentifierName($"{MemberDecl.DeclSymbol.Name}Reserved");
        private SyntaxToken GetCheckpointToken() => SF.Identifier($"{MemberDecl.DeclSymbol.Name}Checkpoint");
        private IdentifierNameSyntax GetCheckpointId() => SF.IdentifierName($"{MemberDecl.DeclSymbol.Name}Checkpoint");
        private SyntaxToken GetDocLengthToken() => SF.Identifier($"{MemberDecl.DeclSymbol.Name}DocLength");
        private IdentifierNameSyntax GetDocLengthId() => SF.IdentifierName($"{MemberDecl.DeclSymbol.Name}DocLength");

        private SyntaxToken GetSizeSpanNameToken() => SF.Identifier($"{MemberDecl.DeclSymbol.Name}SizeSpan");
        private IdentifierNameSyntax GetSizeSpanNameId() => SF.IdentifierName($"{MemberDecl.DeclSymbol.Name}SizeSpan");

        public ArrayWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        private BlockSyntax SimpleForBlock(ReadWriteBase writeOp)
        {

            var elementAccess = SF.ElementAccessExpression(
                                                    Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.Name)),
                                                    SF.BracketedArgumentList().AddArguments(SF.Argument(ForIndexId)));
            return SF.Block(writeOp.GenerateWrite(ClassSymbol, ForIndexId, elementAccess));
        }
        private BlockSyntax GeneratedSerializerForBlock(GeneratedSerializerRW writeOp)
        {
            var elementAccess = SF.ElementAccessExpression(
                                                    Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.Name)),
                                                    SF.BracketedArgumentList().AddArguments(SF.Argument(ForIndexId)));
            return SF.Block(writeOp.GenerateWrite(ClassSymbol, MemberDecl, elementAccess));
        }
        private ForStatementSyntax GenerateFor()
        {
            ITypeSymbol type = MemberDecl.DeclType;
            if (MemberDecl.DeclType.Name.Equals("Nullable"))
            {
                type = MemberDecl.DeclType.TypeArguments[0];
            }
            if (MemberDecl.IsGenericList)
            {
                type = MemberDecl.DeclType.TypeArguments[0];
            }
            TypeMap.TryGetValue(type, out var writeOp);
            BlockSyntax forBody;
            if (writeOp is GeneratedSerializerRW generatedOp)
            {
                forBody = GeneratedSerializerForBlock(generatedOp);
            }
            else
            {
                forBody = SimpleForBlock(writeOp);
            }
            var indexForDeclare = SF.VariableDeclaration(
                                    SF.PredefinedType(SF.Token(SyntaxKind.IntKeyword)),
                                    SF.SingletonSeparatedList(
                                        SF.VariableDeclarator(
                                            ForIndexToken,
                                            default,
                                            SF.EqualsValueClause(SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(0))))));
            var forCondition = SF.BinaryExpression(
                                    kind: SyntaxKind.LessThanExpression,
                                    left: ForIndexId,
                                    right: Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName,
                                                                     SF.IdentifierName(MemberDecl.DeclSymbol.Name),
                                                                     SF.IdentifierName("Count")));
            var incrementExpr = SF.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, ForIndexId);
            return SF.ForStatement(
                    declaration: indexForDeclare,
                    initializers: new SeparatedSyntaxList<ExpressionSyntax>(),
                    condition: forCondition,
                    incrementors: SF.SingletonSeparatedList<ExpressionSyntax>(incrementExpr),
                    statement: forBody);
        }
        StatementSyntax GenerateReserve()
        {
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        Basics.VarTypeIdentifier,
                        SF.SingletonSeparatedList(
                            SF.VariableDeclarator(
                                GetReservedToken(),
                                default,
                                SF.EqualsValueClause(
                                    Basics.InvocationExpression(
                                        Basics.WriterInputVariableIdentifierName,
                                        GetReserveMethodId(),
                                        SF.Argument(default, default, reservedSizeExpr)))))));
        }
        StatementSyntax GenerateWrite_Type_Name()
        {
            return SF.ExpressionStatement(
                            Basics.InvocationExpression(Basics.WriterInputVariableIdentifierName,
                                    SF.IdentifierName("Write_Type_Name"),
                                    SF.Argument(Basics.NumberLiteral(4)),
                                    SF.Argument(SF.IdentifierName(Basics.GenerateReadOnlySpanName(ClassSymbol, MemberDecl)))));
        }
        StatementSyntax GenerateCheckpoint()
        {
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        SF.ParseTypeName("var"),
                        SF.SingletonSeparatedList(
                            SF.VariableDeclarator(
                                GetCheckpointToken(),
                                default,
                                SF.EqualsValueClause(Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("Written")))))));
        }
        StatementSyntax GenerateDocLenDeclareAndAssign()
        {
            var binaryExpr = SF.BinaryExpression(
                                kind: SyntaxKind.SubtractExpression,
                                left: Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, writerWritePropId),
                                right: GetCheckpointId());
            return SF.LocalDeclarationStatement(
                     SF.VariableDeclaration(
                        Basics.VarTypeIdentifier,
                        SF.SingletonSeparatedList(SF.VariableDeclarator(GetDocLengthToken(), default, SF.EqualsValueClause(binaryExpr)))));
        }
        StatementSyntax GenerateWriteEndMarker()
        {
            var endMarkerLiteral = SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal((byte)'\x00'));

            return SF.ExpressionStatement(
                    SF.InvocationExpression(Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("WriteByte")),
                                            SF.ArgumentList().AddArguments(SF.Argument(endMarkerLiteral))));

        }
        StatementSyntax GenerateSizeSpanStackalloc()
        {
            var spanGeneticName = SF.GenericName(SF.Identifier("Span"),
                                                 SF.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(SF.PredefinedType(SF.Token(SyntaxKind.ByteKeyword)))));
            var equalsValueClause = SF.EqualsValueClause(
                                        SF.StackAllocArrayCreationExpression(
                                            SF.ArrayType(
                                                SF.PredefinedType(SF.Token(SyntaxKind.ByteKeyword)),
                                                SF.SingletonList(SF.ArrayRankSpecifier().AddSizes(Basics.NumberLiteral(4))))));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(spanGeneticName, SF.SingletonSeparatedList(SF.VariableDeclarator(GetSizeSpanNameToken(), default, equalsValueClause))));
        }
        public override StatementSyntax Generate()
        {
            return SF.IfStatement(
                        condition: SF.BinaryExpression(
                                        kind: SyntaxKind.EqualsExpression,
                                        left: Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName,
                                                                        SF.IdentifierName(MemberDecl.DeclSymbol.Name)),
                                        operatorToken: SF.Token(SyntaxKind.ExclamationEqualsToken),
                                        right: SF.LiteralExpression(SyntaxKind.NullLiteralExpression, SF.Token(SyntaxKind.NullKeyword))),
                        statement: SF.Block(
                                        GenerateWrite_Type_Name(),
                                        GenerateCheckpoint(),
                                        GenerateReserve(),
                                        GenerateFor(),
                                        GenerateWriteEndMarker(),
                                        GenerateDocLenDeclareAndAssign(),
                                        GenerateSizeSpanStackalloc(),
                                        SF.ExpressionStatement(Basics.InvocationExpression(SF.IdentifierName("BinaryPrimitives"),
                                                                                           SF.IdentifierName("WriteInt32LittleEndian"),
                                                                                           SF.Argument(GetSizeSpanNameId()),
                                                                                           SF.Argument(GetDocLengthId()))),
                                        SF.ExpressionStatement(Basics.InvocationExpression(GetReservedId(), SF.IdentifierName("Write"), SF.Argument(GetSizeSpanNameId()))),
                                        SF.ExpressionStatement(Basics.InvocationExpression0(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("Commit")))),
                            @else: SF.ElseClause(
                                    SF.Block(
                                        SF.ExpressionStatement(
                                            Basics.InvocationExpression(Basics.WriterInputVariableIdentifierName,
                                                SF.IdentifierName("WriteBsonNull"),
                                                SF.Argument(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl)))))));
        }
    }
}
