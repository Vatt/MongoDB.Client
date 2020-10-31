using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal class Int32RW : ReadWriteBase
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SyntaxFactory.IdentifierName("TryGetInt32");

        protected override IdentifierNameSyntax WriteMethodIdentifier => SyntaxFactory.IdentifierName("Write_Type_Name_Value");

        public Int32RW() : base()
        {

        }
    }
}
