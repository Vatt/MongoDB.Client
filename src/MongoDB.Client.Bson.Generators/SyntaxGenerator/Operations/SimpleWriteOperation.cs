using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class SimpleWriteOperation : OperationBase
    {
        public SimpleWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        public override StatementSyntax Generate()
        {
            return SF.ExpressionStatement(
                SF.InvocationExpression(
                    SF.ParseExpression("writer.Write"))
                );
        }
    }
}
