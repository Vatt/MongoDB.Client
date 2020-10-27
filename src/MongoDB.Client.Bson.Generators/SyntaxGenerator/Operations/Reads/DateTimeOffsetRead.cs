using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{

    internal class DateTimeOffsetRead : ReadWithBsonType
    {
        protected override IdentifierNameSyntax MethodIdentifier => SF.IdentifierName("TryGetDateTimeWithBsonType");
        public DateTimeOffsetRead(IdentifierNameSyntax readerIdentifier) : base(readerIdentifier)
        {

        }
    }
}
