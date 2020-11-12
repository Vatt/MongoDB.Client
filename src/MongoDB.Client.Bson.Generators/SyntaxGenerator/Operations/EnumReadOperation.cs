using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SG = MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator.SerializerGenerator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class EnumReadOperation : OperationBase
    {
        public EnumReadOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {
        }

        public override StatementSyntax Generate()// only for string representation
        {
            var assigmentIfTrue = SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, Basics.TryParseOutVariableIdentifier, SG.IdentifierFullName(MemberDecl.DeclSymbol));
            return SF.IfStatement(
                condition: SF.InvocationExpression(
                    expression: SG.SimpleMemberAccess(EnumTryParseMethodDeclaration.VarId, SF.IdentifierName("SequenceEqual")),
                    argumentList: Basics.Arguments(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl))),
                statement: SF.Block(
                    SF.ExpressionStatement(assigmentIfTrue),
                    SF.ReturnStatement(SG.TrueLiteralExpr())));
        }
    }
}
