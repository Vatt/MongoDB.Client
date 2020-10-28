using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class InLoopFieldReadOperation : OperationBase
    {
        public InLoopFieldReadOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        StatementSyntax GenerateMainOperationBlock()
        {
            ReadsMap.TryGetValue(MemberDecl.DeclType, out var readOp);
            readOp.WithMemberAssign(Basics.TryParseOutVariableIdentifier, SF.IdentifierName(MemberDecl.DeclSymbol.Name));
            return SF.IfStatement(
                condition: SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, readOp.Generate()),
                statement: SF.Block(SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
        }
        IfStatementSyntax GenerateIfNameEqualsStatement()
        {
            return SF.IfStatement(
                    condition: SF.InvocationExpression(
                                    expression: Basics.SimpleMemberAccess(Basics.TryParseBsonNameIdentifier, SF.IdentifierName("SequenceEqual")),
                                    argumentList: Basics.Arguments(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl))),
                    statement: SF.Block(GenerateIfBsonTypeNull(),
                                        GenerateMainOperationBlock(),
                                        SF.ContinueStatement())
                  );
        }
        public override StatementSyntax Generate()
        {
            return GenerateIfNameEqualsStatement();
        }
    }
}
