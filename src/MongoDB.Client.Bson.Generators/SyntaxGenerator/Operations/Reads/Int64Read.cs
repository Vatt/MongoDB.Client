using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class Int64Read : ReadBase
    {
        protected override IdentifierNameSyntax MethodIdentifier => SyntaxFactory.IdentifierName("TryGetInt64");
        public Int64Read(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
