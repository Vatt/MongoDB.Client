using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class BsonDocumentRead : ReadWriteBase
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName("TryParseDocument");

        protected override IdentifierNameSyntax WriteMethodIdentifier => throw new System.NotImplementedException();

        public BsonDocumentRead(IdentifierNameSyntax readerVariableName) : base(readerVariableName)
        {
        }


    }
}
