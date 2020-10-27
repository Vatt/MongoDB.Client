using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class InLoopFieldOperation : OperationBase
    {
        public InLoopFieldOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
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
                    condition: SF.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        SF.InvocationExpression(
                            expression: Basics.SimpleMemberAccess(Basics.TryParseBsonNameIdentifier, SF.IdentifierName("SequenceEqual")),
                            argumentList: Basics.Arguments(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl)))
                        ),
                    statement: SF.Block(GenerateIfBsonTypeNull(), 
                                        GenerateMainOperationBlock(),
                                        SF.ContinueStatement())
                  );
        }
        IfStatementSyntax GenerateIfBsonTypeNull()
        {
            return SF.IfStatement(
                    condition: SF.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            Basics.TryParseBsonTypeIdentifier,
                            SF.Token(SyntaxKind.EqualsEqualsToken),
                            Basics.NumberLiteral(10)
                        ),
                    statement: SF.Block(
                        SF.ExpressionStatement(
                            SF.AssignmentExpression(
                                kind: SyntaxKind.SimpleAssignmentExpression,
                                left: Basics.SimpleMemberAccess(Basics.TryParseOutVariableIdentifier, Basics.IdentifierName(MemberDecl.DeclSymbol)),
                                right: SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression))
                            ),
                        SF.ContinueStatement())
                    );


        }
        public override StatementSyntax Generate()
        {
            return GenerateIfNameEqualsStatement();
        }
    }
}
