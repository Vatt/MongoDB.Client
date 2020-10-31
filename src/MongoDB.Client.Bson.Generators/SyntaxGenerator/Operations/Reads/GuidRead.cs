using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class GuidRead : ReadWithBsonType
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName("TryGetGuidWithBsonType");

        protected override IdentifierNameSyntax WriteMethodIdentifier => throw new System.NotImplementedException();

        public GuidRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }

    }
}
