using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations.ReadWrite
{
    internal class GeneratedSerializerRW : ReadWriteBase
    {
        protected INamedTypeSymbol _classSym;
        protected override IdentifierNameSyntax ReadMethodIdentifier => SF.IdentifierName(Basics.GenerateSerializerNameStaticField(_classSym));

        protected override IdentifierNameSyntax WriteMethodIdentifier => ReadMethodIdentifier;


        public GeneratedSerializerRW(INamedTypeSymbol classSym) : base()
        {
            _classSym = classSym;
        }

        public override ArgumentListSyntax ReadArgumentList(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
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
        public override InvocationExpressionSyntax GenerateRead(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            var serializer = Basics.GlobalSerializationHelperGenerated;
            return SF.InvocationExpression(
                           expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            Basics.SimpleMemberAccess(serializer, ReadMethodIdentifier),
                                            SF.IdentifierName("TryParse")),
                           argumentList: ReadArgumentList(classSym, memberDecl));
        }
        public override StatementSyntax GenerateWrite(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            var serializer = Basics.GlobalSerializationHelperGenerated;
            var writeMethod = SF.IdentifierName(Basics.GenerateSerializerNameStaticField(memberDecl.DeclType));
            var serializerInvocation = SF.ExpressionStatement(
                                            SF.InvocationExpression(
                                                       expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Basics.SimpleMemberAccess(serializer, writeMethod), SF.IdentifierName("Write")),
                                                       argumentList: SF.ArgumentList()
                                                                         .AddArguments(
                                                                             SF.Argument(default, SF.Token(SyntaxKind.RefKeyword), Basics.WriterInputVariableIdentifierName),
                                                                             SF.Argument(Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(memberDecl.DeclSymbol.Name))))));
            return SF.IfStatement(
                        condition: SF.BinaryExpression(
                                        kind: SyntaxKind.EqualsExpression,
                                        left: Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName,
                                                                        SF.IdentifierName(memberDecl.DeclSymbol.Name)),
                                        operatorToken: SF.Token(SyntaxKind.ExclamationEqualsToken),
                                        right: SF.LiteralExpression(SyntaxKind.NullLiteralExpression, SF.Token(SyntaxKind.NullKeyword))),
                        statement: SF.Block(
                                        SF.ExpressionStatement(
                                            Basics.InvocationExpression(Basics.WriterInputVariableIdentifierName,
                                                                            SF.IdentifierName("Write_Type_Name"),
                                                                            SF.Argument(Basics.NumberLiteral(3)),
                                                                            SF.Argument(SF.IdentifierName(Basics.GenerateReadOnlySpanName(classSym, memberDecl))))),
                                        serializerInvocation),
                            @else: SF.ElseClause(
                                    SF.ExpressionStatement(
                                        Basics.InvocationExpression(Basics.WriterInputVariableIdentifierName,
                                            SF.IdentifierName("WriteBsonNull"),
                                            SF.Argument(Basics.GenerateReadOnlySpanNameIdentifier(classSym, memberDecl))))));
        }


    }
}
