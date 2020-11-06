using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class EnumTryParseMethodDeclaration : SimpleTryParseMethodDeclaration
    {
        internal static SyntaxToken VarToken = SF.Identifier("enumValue");
        internal static IdentifierNameSyntax VarId = SF.IdentifierName("enumValue");
        public EnumTryParseMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl)
        {
        }

        public override BlockSyntax GenerateMethodBody()
        {
            var variableDecl = SF.DeclarationExpression(SF.IdentifierName("ReadOnlySpan<byte>"), SF.SingleVariableDesignation(VarToken));
            var invocation = Basics.InvocationExpression(
                                    Basics.ReaderInputVariableIdentifier,
                                    SF.IdentifierName("TryGetStringAsSpan"),                                     
                                    SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), variableDecl));
            var defaultAssigment = SF.AssignmentExpression(
                                        SyntaxKind.SimpleAssignmentExpression, 
                                        Basics.TryParseOutVariableIdentifier,
                                        SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression));
            return SF.Block(
                      SF.ExpressionStatement(defaultAssigment),  
                      SF.ExpressionStatement(invocation))
                     .AddStatements(OperationsList.CreateReadOperations(ClassSymbol, Members).Generate())
                     .AddStatements(SF.ParseStatement(@$"throw new ArgumentException($""{ClassSymbol.Name}.TryParse  with enum string value {{System.Text.Encoding.UTF8.GetString(enumValue)}}"");"));
        }
    }
}