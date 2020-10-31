using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.Reads
{
    internal class GeneratedSerializerRead : ReadWriteBase
    {
        private INamedTypeSymbol _classSymbol;
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName(Basics.GenerateSerializerNameStaticField(_classSymbol));

        protected override IdentifierNameSyntax WriteMethodIdentifier => throw new System.NotImplementedException();

        public GeneratedSerializerRead(INamedTypeSymbol classSymbol, IdentifierNameSyntax readerVariableName) : base(readerVariableName)
        {
            _classSymbol = classSymbol;
        }

        public override ArgumentListSyntax ArgumentList()
        {
            if (_variableDecl != null)
            {
                return SF.ArgumentList(
                    new SeparatedSyntaxList<ArgumentSyntax>()
                        .Add(SF.Argument(default, SF.Token(SyntaxKind.RefKeyword), Basics.ReaderInputVariableIdentifier))
                        .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _variableDecl)));
            }
            else if (_assignExpr != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
            }
            return default;
        }
        public override InvocationExpressionSyntax GenerateRead()
        {
            var serializer = Basics.GlobalSerializationHelperGenerated;
            return SF.InvocationExpression(
                           expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            Basics.SimpleMemberAccess(serializer, ReadMethodIdentifier),
                                            SF.IdentifierName("TryParse")),
                           argumentList: ArgumentList());
        }


    }
}
