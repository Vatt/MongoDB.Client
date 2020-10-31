using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal class BsonDocumentRW : ReadWriteBase
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName("TryParseDocument");

        protected override IdentifierNameSyntax WriteMethodIdentifier => SF.IdentifierName("Write_Type_Name");

        public BsonDocumentRW(IdentifierNameSyntax readerVariableName) : base(readerVariableName)
        {
        }


    }
}
