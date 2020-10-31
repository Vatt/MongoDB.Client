using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class GeneratedSerializerWriteOperation : OperationBase
    {
        public GeneratedSerializerWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        public override StatementSyntax Generate()
        {
            var serializer = Basics.GlobalSerializationHelperGenerated;
            var writeMethod = SF.IdentifierName(Basics.GenerateSerializerNameStaticField(MemberDecl.DeclType));
            var serializerInvocation = SF.ExpressionStatement(
                                            SF.InvocationExpression(
                                                       expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Basics.SimpleMemberAccess(serializer, writeMethod), SF.IdentifierName("Write")),
                                                       argumentList: SF.ArgumentList()
                                                                         .AddArguments(
                                                                             SF.Argument(default, SF.Token(SyntaxKind.RefKeyword), Basics.WriterInputVariableIdentifierName),
                                                                             SF.Argument(Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.Name))))));
            return SF.IfStatement(
                        condition: SF.BinaryExpression(
                                        kind: SyntaxKind.EqualsExpression,
                                        left: Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName,
                                                                        SF.IdentifierName(MemberDecl.DeclSymbol.Name)),
                                        operatorToken: SF.Token(SyntaxKind.ExclamationEqualsToken),
                                        right: SF.LiteralExpression(SyntaxKind.NullLiteralExpression, SF.Token(SyntaxKind.NullKeyword))),
                        statement: SF.Block(
                                        SF.ExpressionStatement(
                                            Basics.InvocationExpression(Basics.WriterInputVariableIdentifierName,
                                                                            SF.IdentifierName("Write_Type_Name"),
                                                                            SF.Argument(Basics.NumberLiteral(3)),
                                                                            SF.Argument(SF.IdentifierName(Basics.GenerateReadOnlySpanName(ClassSymbol, MemberDecl))))),
                                        serializerInvocation),
                            @else: SF.ElseClause(
                                    SF.ExpressionStatement(
                                        Basics.InvocationExpression(Basics.WriterInputVariableIdentifierName,
                                            SF.IdentifierName("WriteBsonNull"),
                                            SF.Argument(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl))))));

        }
    }
}
