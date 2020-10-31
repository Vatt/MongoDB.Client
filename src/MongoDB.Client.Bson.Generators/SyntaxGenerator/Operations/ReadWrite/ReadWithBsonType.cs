using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal abstract class ReadWithBsonType : ReadWriteBase
    {
        public ReadWithBsonType() : base()
        {

        }
        public override ArgumentListSyntax ReadArgumentList(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            if (_assignExpr != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                    .Add(SF.Argument(Basics.TryParseBsonTypeIdentifier))
                    .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
            }
            else if (_variableDecl != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                    .Add(SF.Argument(Basics.TryParseBsonTypeIdentifier))
                    .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _variableDecl)));
            }
            return default;

        }
    }
}
