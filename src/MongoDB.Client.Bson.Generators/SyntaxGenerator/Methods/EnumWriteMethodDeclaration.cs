using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class EnumWriteMethodDeclaration : SimpleWriteMethodDeclaration
    {
        public EnumWriteMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl)
        {

        }

        public override BlockSyntax GenerateMethodBody()
        {
            return SF.Block( OperationsList.CreateWriteOperations(ClassSymbol, Members).Generate());
        }
    }
}
