using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal abstract class ReadWithBsonType : ReadWriteBase
    {
        protected IdentifierNameSyntax _typeId;
        public ReadWithBsonType() : base()
        {

        }
        public void SetBsonType(IdentifierNameSyntax typeId)
        {
            _typeId = typeId;
        }
        public override ArgumentListSyntax ReadArgumentList(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            if (_assignExpr != null)
            {
                var args =  SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                                .Add(SF.Argument(_typeId/*Basics.TryParseBsonTypeIdentifier*/))
                                .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
                _typeId = null;
                return args;
            }
            else if (_variableDecl != null)
            {
                var args =  SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                                .Add(SF.Argument(_typeId/*Basics.TryParseBsonTypeIdentifier*/))
                                .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _variableDecl)));
                _typeId = null;
                return args;
            }
            return default;

        }
    }
}
