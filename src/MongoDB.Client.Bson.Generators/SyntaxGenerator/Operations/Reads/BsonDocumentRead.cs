using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class BsonDocumentRead : ReadBase
    {
        protected override IdentifierNameSyntax MethodIdentifier => SF.IdentifierName("TryParseDocument");
        public BsonDocumentRead(IdentifierNameSyntax readerVariableName) : base(readerVariableName)
        {
        }


    }
}
