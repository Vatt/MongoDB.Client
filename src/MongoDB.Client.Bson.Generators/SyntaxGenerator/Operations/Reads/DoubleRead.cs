using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class DoubleRead : ReadBase
    {
        protected override IdentifierNameSyntax MethodIdentifier => SyntaxFactory.IdentifierName("TryGetDouble");
        public DoubleRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
