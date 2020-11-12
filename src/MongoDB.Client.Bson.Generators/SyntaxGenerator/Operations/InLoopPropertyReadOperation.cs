using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SG = MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator.SerializerGenerator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class InLoopPropertyReadOperation : OperationBase
    {
        private string _variadleIdentifier => "value";
        public InLoopPropertyReadOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        StatementSyntax GenerateMainOperationBlock()
        {
            ITypeSymbol type = MemberDecl.DeclType;
            if (MemberDecl.DeclType.Name.Equals("Nullable"))
            {
                type = MemberDecl.DeclType.TypeArguments[0];
            }
            TypeMap.TryGetValue(type/*MemberDecl.DeclType*/, out var readOp);
            readOp.WithVariableDeclaration(_variadleIdentifier);
            if (readOp is ReadWithBsonType rwWithType)
            {
                rwWithType.SetBsonType(Basics.TryParseBsonTypeIdentifier);
            }
            return SF.IfStatement(
                condition: SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, readOp.GenerateRead(ClassSymbol, MemberDecl)),
                statement: SF.Block(SF.ReturnStatement(SF.LiteralExpression(SyntaxKind.FalseLiteralExpression))));
        }
        StatementSyntax GenerateAssignForTempVariable()
        {
            return SF.ExpressionStatement(SF.AssignmentExpression(
                                            kind: SyntaxKind.SimpleAssignmentExpression,
                                            left: SG.SimpleMemberAccess(Basics.TryParseOutVariableIdentifier, MemberDecl),
                                            right: SF.IdentifierName(_variadleIdentifier)));
        }
        IfStatementSyntax GenerateIfNameEqualsStatement()
        {
            return SF.IfStatement(
                    condition: SF.InvocationExpression(
                                    expression: SG.SimpleMemberAccess(Basics.TryParseBsonNameIdentifier, SF.IdentifierName("SequenceEqual")),
                                    argumentList: Basics.Arguments(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl))),
                    statement: SF.Block(GenerateIfBsonTypeNull(),
                                        GenerateMainOperationBlock(),
                                        GenerateAssignForTempVariable(),
                                        SF.ContinueStatement())
                  );
        }

        public override StatementSyntax Generate()
        {
            return GenerateIfNameEqualsStatement();
        }
    }
}
