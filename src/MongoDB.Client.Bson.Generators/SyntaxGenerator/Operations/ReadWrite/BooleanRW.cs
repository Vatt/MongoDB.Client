using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal class BooleanRW : ReadWriteBase
    {
        protected override IdentifierNameSyntax ReadMethodIdentifier => SyntaxFactory.IdentifierName("TryGetBoolean");

        protected override IdentifierNameSyntax WriteMethodIdentifier => SyntaxFactory.IdentifierName("Write_Type_Name_Value");

        public BooleanRW() : base()
        {

        }
    }
}
