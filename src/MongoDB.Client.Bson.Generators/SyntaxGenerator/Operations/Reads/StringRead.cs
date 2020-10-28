using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class StringRead : ReadBase
    {
        protected override IdentifierNameSyntax MethodIdentifier => SyntaxFactory.IdentifierName("TryGetString");
        public StringRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
