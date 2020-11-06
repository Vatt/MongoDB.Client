using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class EnumWriteOperation : OperationBase
    {
        public EnumWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {
        }

        public override StatementSyntax Generate()
        {
            var a = 10;
            var b = 11;
            if (a == b) { }
            return SF.IfStatement(
                condition: SF.BinaryExpression(SyntaxKind.EqualsExpression, Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.ToString())),
                statement: SF.Block(
                    SF.ExpressionStatement(
                                 Basics.InvocationExpression(
                                     Basics.WriterInputVariableIdentifierName, 
                                     SF.IdentifierName("WriteString"),
                                 SF.Argument(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol,MemberDecl))))));
        }
    }
}
