using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class ObjectIdRead : ReadBase
    {
        protected override IdentifierNameSyntax MethodIdentifier => SyntaxFactory.IdentifierName("TryGetObjectId");
        public ObjectIdRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
