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
        protected abstract IdentifierNameSyntax ReadMethodIdentifier { get; }
        protected abstract IdentifierNameSyntax WriteMethodIdentifier { get; }
        public ReadWriteBase()
        {

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
        public virtual ArgumentListSyntax ReadArgumentList(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
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
        public virtual InvocationExpressionSyntax GenerateRead(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {

            var expr = SF.InvocationExpression(
                               expression: Basics.SimpleMemberAccess(Basics.ReaderInputVariableIdentifier, ReadMethodIdentifier),
                               argumentList: ReadArgumentList(classSym, memberDecl));
            _variableDecl = null;
            _assignExpr = null;
            return expr;

        }
        public virtual StatementSyntax GenerateRead(ExpressionSyntax outVarExpr)
        {
            return SF.ExpressionStatement(
                    SF.InvocationExpression(
                        Basics.SimpleMemberAccess(Basics.ReaderInputVariableIdentifier, ReadMethodIdentifier),
                        SF.ArgumentList().AddArguments(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), outVarExpr))));
        }
        public virtual StatementSyntax GenerateWrite(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            return GenerateWrite(classSym, memberDecl, Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(memberDecl.DeclSymbol.Name)));
        }
        public virtual StatementSyntax GenerateWrite(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl, ExpressionSyntax writableVar)
        {
            return GenerateWrite(classSym, SF.IdentifierName(Basics.GenerateReadOnlySpanName(classSym, memberDecl)), writableVar);
        }
        public virtual StatementSyntax GenerateWrite(INamedTypeSymbol classSym, ExpressionSyntax nameExpr, ExpressionSyntax writableVar)
        {
            return SF.ExpressionStatement(
                    SF.InvocationExpression(
                        Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, WriteMethodIdentifier),
                        SF.ArgumentList()
                            .AddArguments(
                                SF.Argument(nameExpr),
                                SF.Argument(writableVar))));
        }
    }
}
