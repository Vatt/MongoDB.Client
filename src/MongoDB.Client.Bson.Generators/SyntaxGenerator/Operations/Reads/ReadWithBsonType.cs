using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal abstract class ReadWithBsonType : ReadBase
    {
        public ReadWithBsonType(IdentifierNameSyntax readerVariableName) : base(readerVariableName)
        {

        }
        public override ArgumentListSyntax ArgumentList()
        {
            return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                            .Add(SF.Argument(GeneratorBasics.TryParseBsonTypeIdentifier))
                            .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
        }
    }
}
