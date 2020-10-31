using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal class GuidRW : ReadWithBsonType
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName("TryGetGuidWithBsonType");

        protected override IdentifierNameSyntax WriteMethodIdentifier => SF.IdentifierName("Write_Type_Name");

        public GuidRW(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }

    }
}
