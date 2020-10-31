using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{

    internal class DateTimeOffsetRW : ReadWithBsonType
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName("TryGetDateTimeWithBsonType");

        protected override IdentifierNameSyntax WriteMethodIdentifier => SF.IdentifierName("Write_Type_Name");

        public DateTimeOffsetRW(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
