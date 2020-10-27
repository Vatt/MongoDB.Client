using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class FieldOperation : OperationBase
    {
        public FieldOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        StatementSyntax GenerateMainOperationBlock()
        {
            ReadsMap.TryGetValue(MemberDecl.DeclType, out var readOp);
            readOp.WithMemberAssign(GeneratorBasics.TryParseOutVariableIdentifier, SF.IdentifierName(MemberDecl.DeclSymbol.Name));
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
                            expression: GeneratorBasics.SimpleMemberAccess(GeneratorBasics.TryParseBsonNameIdentifier, SF.IdentifierName("SequenceEqual")),
                            argumentList: GeneratorBasics.Arguments(GeneratorBasics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl)))
                        ),
                    statement: SF.Block(GenerateIfBsonTypeNull(), GenerateMainOperationBlock())
                  );
        }
        IfStatementSyntax GenerateIfBsonTypeNull()
        {
            return SF.IfStatement(
                    condition: SF.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            GeneratorBasics.TryParseBsonTypeIdentifier,
                            SF.Token(SyntaxKind.EqualsEqualsToken),
                            GeneratorBasics.NumberLiteral(10)
                        ),
                    statement: SF.Block(
                        SF.ExpressionStatement(
                            SF.AssignmentExpression(
                                kind: SyntaxKind.SimpleAssignmentExpression,
                                left: GeneratorBasics.SimpleMemberAccess(GeneratorBasics.TryParseOutVariableIdentifier, GeneratorBasics.IdentifierName(MemberDecl.DeclSymbol)),
                                right: SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression))
                            )
                        )
                    );


        }
        public override StatementSyntax Generate()
        {
            return GenerateIfNameEqualsStatement();
        }
    }
}
