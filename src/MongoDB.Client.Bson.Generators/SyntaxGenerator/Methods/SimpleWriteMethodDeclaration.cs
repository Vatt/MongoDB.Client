using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using System;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class SimpleWriteMethodDeclaration : WriteMethodDeclarationBase
    {
        public static SyntaxToken checkpointId = SF.Identifier("checkpoint");
        public static SyntaxToken reservedId = SF.Identifier("reserved");
        public static ExpressionSyntax reservedSizeExpr = SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(4));
        public SimpleWriteMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl.ClassSymbol, classdecl.Members)
        {

        }


        public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier()
        {
            return SF.ExplicitInterfaceSpecifier(
                   SF.GenericName(
                       Basics.SerializerInterfaceIdentifier,
                       SF.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(SF.ParseTypeName(ClassSymbol.Name)))),
                   SF.Token(SyntaxKind.DotToken));
        }
        StatementSyntax GenerateCheckpoint()
        {
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        SF.ParseTypeName("var"),
                        SF.SingletonSeparatedList(
                            SF.VariableDeclarator(
                                checkpointId, 
                                default,
                                SF.EqualsValueClause(Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("Written")))))));
        }
        StatementSyntax GenerateReserve()
        {
            return SF.LocalDeclarationStatement(
                    SF.VariableDeclaration(
                        SF.ParseTypeName("var"),
                        SF.SingletonSeparatedList(
                            SF.VariableDeclarator(
                                reservedId,
                                default,
                                SF.EqualsValueClause(
                                    Basics.InvocationExpression1(
                                        Basics.WriterInputVariableIdentifierName,
                                        SF.IdentifierName("Reserve"),
                                        SF.Argument(default, default, reservedSizeExpr)))))));
        }


        public override TypeSyntax GetWriteMethodInParameter() => SF.ParseTypeName(ClassSymbol.Name);

        public override BlockSyntax GenerateMethodBody()
        {
            return SF.Block(
                    GenerateCheckpoint(),
                    GenerateReserve())
                    .AddStatements(OperationsList.CreateWriteOperations(ClassSymbol, Members).Generate())
                    .AddStatements(
                        SF.ParseStatement("writer.WriteByte( (byte)'\\x00');"),
                        SF.ParseStatement("var docLength = writer.Written - checkpoint;"),
                        SF.ParseStatement("Span<byte> sizeSpan = stackalloc byte[4];"),
                        SF.ParseStatement("BinaryPrimitives.WriteInt32LittleEndian(sizeSpan, docLength);"),
                        SF.ParseStatement("reserved.Write(sizeSpan);"),
                        SF.ParseStatement("writer.Commit();"));
        }
    }
}
