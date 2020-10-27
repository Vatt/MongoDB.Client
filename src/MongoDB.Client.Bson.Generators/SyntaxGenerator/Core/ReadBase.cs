using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class ReadBase
    {
        protected DeclarationExpressionSyntax _variableDecl;
        protected ExpressionSyntax _assignExpr;
        protected IdentifierNameSyntax _readerVariableName;

        protected abstract IdentifierNameSyntax MethodIdentifier { get; }
        public ReadBase(IdentifierNameSyntax readerVariableName)
        {
            _readerVariableName = readerVariableName;
        }
        public void WithVariableDeclaration(string identifier)
        {
            _variableDecl = SF.DeclarationExpression(SF.IdentifierName("var"), //SF.Token(SyntaxKind.VarKeyword)
                                                     SF.SingleVariableDesignation(SF.Identifier(identifier)));
        }
        public void WithVariableAssign()
        {

        }
        public void WithMemberAssign(IdentifierNameSyntax source, IdentifierNameSyntax member)
        {
            _assignExpr = Basics.SimpleMemberAccess(source, member);
        }
        public virtual ArgumentListSyntax ArgumentList()
        {
            if(_variableDecl != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _variableDecl)));
            }
            else if(_assignExpr != null)
            {
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
            }
            return default;
            
        }
        public virtual InvocationExpressionSyntax Generate()
        {
            return SF.InvocationExpression(
                       expression: Basics.SimpleMemberAccess(_readerVariableName, MethodIdentifier),
                       argumentList: ArgumentList());

        }
    }
}
