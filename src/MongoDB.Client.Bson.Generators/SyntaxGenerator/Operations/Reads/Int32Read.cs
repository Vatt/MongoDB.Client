using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class Int32Read : ReadWriteBase
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SyntaxFactory.IdentifierName("TryGetInt32");

        protected override IdentifierNameSyntax WriteMethodIdentifier => throw new System.NotImplementedException();

        public Int32Read(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
