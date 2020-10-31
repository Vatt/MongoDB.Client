using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{

    internal class DateTimeOffsetRead : ReadWithBsonType
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName("TryGetDateTimeWithBsonType");

        protected override IdentifierNameSyntax WriteMethodIdentifier => throw new System.NotImplementedException();

        public DateTimeOffsetRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
