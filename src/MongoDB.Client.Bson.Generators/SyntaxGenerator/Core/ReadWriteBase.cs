using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class ReadWriteBase
    {
        protected DeclarationExpressionSyntax _variableDecl;
        protected ExpressionSyntax _assignExpr;
        protected IdentifierNameSyntax _readerVariableName;

        protected abstract IdentifierNameSyntax ReadMethodIdentifier { get; }
        protected abstract IdentifierNameSyntax WriteMethodIdentifier { get; }
        public ReadWriteBase(IdentifierNameSyntax readerVariableName)
        {
            _readerVariableName = readerVariableName;
        }
        public void WithVariableDeclaration(string identifier)
        {
            _variableDecl = SF.DeclarationExpression(SF.IdentifierName("var"),
                                                     SF.SingleVariableDesignation(SF.Identifier(identifier)));
        }
        public void WithVariableAssign()
        {
            Debugger.Break();
        }
        public void WithMemberAssign(IdentifierNameSyntax source, IdentifierNameSyntax member)
        {
            _assignExpr = Basics.SimpleMemberAccess(source, member);
        }
        public virtual ArgumentListSyntax ArgumentList()
        {
            if (_variableDecl != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _variableDecl)));
            }
            else if (_assignExpr != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
            }
            return default;

        }
        public virtual InvocationExpressionSyntax GenerateRead()
        {

            var expr = SF.InvocationExpression(
                               expression: Basics.SimpleMemberAccess(_readerVariableName, ReadMethodIdentifier),
                               argumentList: ArgumentList());
            _variableDecl = null;
            _assignExpr = null;
            return expr;

        }
    }
}
