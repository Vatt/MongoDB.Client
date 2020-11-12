using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SG = MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator.SerializerGenerator;

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
                return SF.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>()
                        .Add(SF.Argument(default, SF.Token(SyntaxKind.RefKeyword), Basics.ReaderInputVariableIdentifier))
                        .Add(SF.Argument(default, SF.Token(SyntaxKind.OutKeyword), _assignExpr)));
            }
            return default;
        }
        public override InvocationExpressionSyntax GenerateRead(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            var serializer = Basics.GlobalSerializationHelperGenerated;
            return SF.InvocationExpression(
                           expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            SG.SimpleMemberAccess(serializer, ReadMethodIdentifier),
                                            SF.IdentifierName("TryParse")),
                           argumentList: ReadArgumentList(classSym, memberDecl));
        }
        public override StatementSyntax GenerateWrite(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl)
        {
            return GenerateWrite(classSym, memberDecl, SG.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(memberDecl.DeclSymbol.Name)));
        }

        public int GenerateBsonType(INamedTypeSymbol sym)
        {
            if (sym.TypeKind == TypeKind.Enum)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        public override StatementSyntax GenerateWrite(INamedTypeSymbol classSym, MemberDeclarationMeta memberDecl, ExpressionSyntax writableVar)
        {
            var serializer = Basics.GlobalSerializationHelperGenerated;
            IdentifierNameSyntax writeMethod;
            if (memberDecl.IsGenericList)
            {
                writeMethod = SF.IdentifierName(Basics.GenerateSerializerNameStaticField(memberDecl.DeclType.TypeArguments[0]));
            }
            else
            {
                writeMethod = SF.IdentifierName(Basics.GenerateSerializerNameStaticField(memberDecl.DeclType));
            }

            var serializerInvocation = SF.ExpressionStatement(
                                            SF.InvocationExpression(
                                                       expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SG.SimpleMemberAccess(serializer, writeMethod), SF.IdentifierName("Write")),
                                                       argumentList: SF.ArgumentList()
                                                                         .AddArguments(
                                                                             SF.Argument(default, SF.Token(SyntaxKind.RefKeyword), Basics.WriterInputVariableIdentifierName),
                                                                             SF.Argument(writableVar))));
            return SF.IfStatement(
                        condition: SF.BinaryExpression(
                                        kind: SyntaxKind.EqualsExpression,
                                        left: SG.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName,
                                                                        SF.IdentifierName(memberDecl.DeclSymbol.Name)),
                                        operatorToken: SF.Token(SyntaxKind.ExclamationEqualsToken),
                                        right: SF.LiteralExpression(SyntaxKind.NullLiteralExpression, SF.Token(SyntaxKind.NullKeyword))),
                        statement: SF.Block(
                                        SF.ExpressionStatement(
                                            SG.Write_Type_Name(GenerateBsonType(memberDecl.DeclType),
                                                               SG.ReadOnlySpanNameIdentifier(classSym, memberDecl))),
                                        serializerInvocation),
                        @else: SF.ElseClause(
                            SF.Block(SF.ExpressionStatement(
                                     SG.WriteBsonNull(Basics.GenerateReadOnlySpanNameIdentifier(classSym, memberDecl))))));
        }
        public override StatementSyntax GenerateWrite(INamedTypeSymbol classSym, ExpressionSyntax nameExpr, ExpressionSyntax writableVar)
        {
            throw new NotSupportedException();
        }
    }
}
