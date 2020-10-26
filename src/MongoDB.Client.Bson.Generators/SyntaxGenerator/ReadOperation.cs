using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    class ReadOperation
    {
        INamedTypeSymbol ClassSymbol;
        MemberDeclarationMeta MemberDecl;
        IfStatementSyntax _ifBsonTypeNull;
        public ReadOperation(INamedTypeSymbol classSym, MemberDeclarationMeta member)
        {
            ClassSymbol = classSym;
            MemberDecl = member;
            _ifBsonTypeNull = GenerateIfBsonTypeNull();
        }
        IfStatementSyntax GenerateIfNameEqualsStatement()
        {
            //ReadOpsMethodIdentifiers.TryGetValue(MemberDecl.DeclType, out var methodId);
            return SF.IfStatement(
                    condition: SF.PrefixUnaryExpression(
                        SyntaxKind.LogicalNotExpression,
                        SF.InvocationExpression(
                            expression: GeneratorBasics.SimpleMemberAccess(GeneratorBasics.TryParseBsonNameIdentifier,
                                                                                     SF.IdentifierName("SequenceEquals")),
                            argumentList: GeneratorBasics.Arguments(GeneratorBasics.GenerateReadOnlySpanNameIdentifier(ClassSymbol, MemberDecl)))
                        ),
                    statement: SF.Block(GenerateIfBsonTypeNull())
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
                                left: GeneratorBasics.SimpleMemberAccess(GeneratorBasics.TryParseOutputVariableIdentifierName,
                                                                                   GeneratorBasics.IdentifierName(MemberDecl.DeclSymbol)),
                                right: SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression))
                            )
                        )
                    );


        }
        public StatementSyntax Build()
        {
            return GenerateIfNameEqualsStatement();
        }
    }
}
