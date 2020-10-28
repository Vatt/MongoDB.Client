using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class BooleanRead : ReadBase
    {
        protected override IdentifierNameSyntax MethodIdentifier => SyntaxFactory.IdentifierName("TryGetBoolean");
        public BooleanRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
