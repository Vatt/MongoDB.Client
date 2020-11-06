using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class SimpleWriteMethodDeclaration : WriteMethodDeclarationBase
    {
        public static SyntaxToken checkpointToken = SF.Identifier("checkpoint");
        public static IdentifierNameSyntax checkpointId = SF.IdentifierName("checkpoint");
        public static SyntaxToken reservedToken = SF.Identifier("reserved");
        public static IdentifierNameSyntax reservedId = SF.IdentifierName("reserved");
        public static IdentifierNameSyntax writeByteMethodId = SF.IdentifierName("WriteByte");
        public static IdentifierNameSyntax writerWritePropId = SF.IdentifierName("Written");
        public static IdentifierNameSyntax docLengthId = SF.IdentifierName("docLength");
        public static SyntaxToken docLengthToken = SF.Identifier("docLength");
        public static SyntaxToken sizeSpanNameToken = SF.Identifier("sizeSpan");
        public static IdentifierNameSyntax sizeSpanNameId = SF.IdentifierName("sizeSpan");
        public static ExpressionSyntax reservedSizeExpr = SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(4));
        public static IdentifierNameSyntax commitId = SF.IdentifierName("Commit");
        public SimpleWriteMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl.ClassSymbol, classdecl.Members)
        {

        }


        public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier()
        {
            return SF.ExplicitInterfaceSpecifier(
                   SF.GenericName(
                       Basics.SerializerInterfaceIdentifier,
                       SF.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(SF.ParseTypeName(ClassSymbol.ToString())))),
                   SF.Token(SyntaxKind.DotToken));
        }
        protected virtual StatementSyntax GenerateCheckpoint()
        {
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        SF.ParseTypeName("var"),
                        SF.SingletonSeparatedList(
                            SF.VariableDeclarator(
                                checkpointToken,
                                default,
                                SF.EqualsValueClause(Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("Written")))))));
        }
        protected virtual StatementSyntax GenerateReserve()
        {
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        Basics.VarTypeIdentifier,
                        SF.SingletonSeparatedList(
                            SF.VariableDeclarator(
                                reservedToken,
                                default,
                                SF.EqualsValueClause(
                                    Basics.InvocationExpression(
                                        Basics.WriterInputVariableIdentifierName,
                                        SF.IdentifierName("Reserve"),
                                        SF.Argument(default, default, reservedSizeExpr)))))));
        }
        protected virtual StatementSyntax GenerateWriteEndMarker()
        {
            var endMarkerLiteral = SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal((byte)'\x00'));

            return SF.ExpressionStatement(
                    SF.InvocationExpression(Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, writeByteMethodId),
                                            SF.ArgumentList().AddArguments(SF.Argument(endMarkerLiteral))));

        }
        protected virtual StatementSyntax GenerateDocLenDeclareAndAssign()
        {
            var binaryExpr = SF.BinaryExpression(
                kind: SyntaxKind.SubtractExpression,
                left: Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, writerWritePropId),
                right: checkpointId);
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        Basics.VarTypeIdentifier,
                        SF.SingletonSeparatedList(SF.VariableDeclarator(docLengthToken, default, SF.EqualsValueClause(binaryExpr)))));
        }
        protected virtual StatementSyntax GenerateSizeSpanStackalloc()
        {
            var spanGeneticName = SF.GenericName(SF.Identifier("Span"),
                                                 SF.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(SF.PredefinedType(SF.Token(SyntaxKind.ByteKeyword)))));
            var equalsValueClause = SF.EqualsValueClause(
                                        SF.StackAllocArrayCreationExpression(
                                            SF.ArrayType(
                                                SF.PredefinedType(SF.Token(SyntaxKind.ByteKeyword)),
                                                SF.SingletonList(SF.ArrayRankSpecifier().AddSizes(Basics.NumberLiteral(4))))));
            return SF.LocalDeclarationStatement(SF.VariableDeclaration(spanGeneticName, SF.SingletonSeparatedList(SF.VariableDeclarator(sizeSpanNameToken, default, equalsValueClause))));
        }
        protected virtual StatementSyntax GenerateWriteSizeIntoLocalSpan()
        {
            return SF.ExpressionStatement(Basics.InvocationExpression(SF.IdentifierName("BinaryPrimitives"), SF.IdentifierName("WriteInt32LittleEndian"), SF.Argument(sizeSpanNameId), SF.Argument(docLengthId)));
        }
        protected virtual StatementSyntax GenerateReservedWriteIntoSizeSpan()
        {
            return SF.ExpressionStatement(Basics.InvocationExpression(reservedId, SF.IdentifierName("Write"), SF.Argument(sizeSpanNameId)));
        }
        protected virtual StatementSyntax GenerateWriterCommit()
        {
            return SF.ExpressionStatement(Basics.InvocationExpression0(Basics.WriterInputVariableIdentifierName, commitId));
        }
        public override TypeSyntax GetWriteMethodInParameter() => SF.ParseTypeName(ClassSymbol.ToString());

        public override BlockSyntax GenerateMethodBody()
        {
            return SF.Block(
                    GenerateCheckpoint(),
                    GenerateReserve())
                    .AddStatements(OperationsList.CreateWriteOperations(ClassSymbol, Members).Generate())
                    .AddStatements(
                        GenerateWriteEndMarker(),//SF.ParseStatement("writer.WriteByte( (byte)'\\x00');"),
                        GenerateDocLenDeclareAndAssign(),//SF.ParseStatement("var docLength = writer.Written - checkpoint;"),
                        GenerateSizeSpanStackalloc(),//SF.ParseStatement("Span<byte> sizeSpan = stackalloc byte[4];"),
                        GenerateWriteSizeIntoLocalSpan(),//SF.ParseStatement("BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);"),
                        GenerateReservedWriteIntoSizeSpan(),//SF.ParseStatement("reserved.Write(sizeSpan);"),
                        GenerateWriterCommit());// SF.ParseStatement("writer.Commit();"));
        }
    }
}
