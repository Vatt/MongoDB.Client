using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class GuidRead : ReadWithBsonType
    {
        protected override IdentifierNameSyntax MethodIdentifier => SF.IdentifierName("TryGetGuidWithBsonType");
        public GuidRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }

    }
}
