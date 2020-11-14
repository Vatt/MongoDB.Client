using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SG = MongoDB.Client.Bson.Generators.SyntaxGenerator.Generator.SerializerGenerator;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Core
{
    internal abstract class OperationBase
    {
        public static System.Collections.Generic.List<ClassDeclMeta> meta;
        public INamedTypeSymbol ClassSymbol { get; }
        public MemberDeclarationMeta MemberDecl { get; }

        public OperationBase(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl)
        {
            ClassSymbol = classsymbol;
            MemberDecl = memberdecl;
        }
        protected virtual IfStatementSyntax GenerateIfBsonTypeNull()
        {
            return SF.IfStatement(
                    condition: SF.BinaryExpression(
                            SyntaxKind.EqualsExpression,
                            Basics.TryParseBsonTypeIdentifier,
                            SF.Token(SyntaxKind.EqualsEqualsToken),
                            SG.NumericLiteralExpr(10)
                        ),
                    statement: SF.Block(
                        SF.ExpressionStatement(
                            SF.AssignmentExpression(
                                kind: SyntaxKind.SimpleAssignmentExpression,
                                left: SG.SimpleMemberAccess(Basics.TryParseOutVariableIdentifier, Basics.IdentifierName(MemberDecl.DeclSymbol)),
                                right: SG.DefaultLiteralExpr())
                            ),
                        SF.ContinueStatement())
                    );


        }


        public abstract StatementSyntax Generate();
    }
}
