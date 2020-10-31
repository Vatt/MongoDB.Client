using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal class ObjectIdRW : ReadWriteBase
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SyntaxFactory.IdentifierName("TryGetObjectId");

        protected override IdentifierNameSyntax WriteMethodIdentifier => SyntaxFactory.IdentifierName("Write_Type_Name_Value");

        public ObjectIdRW() : base()
        {

        }
    }
}
