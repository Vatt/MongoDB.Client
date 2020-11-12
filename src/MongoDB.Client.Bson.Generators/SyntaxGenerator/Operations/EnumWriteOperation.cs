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
    internal class EnumWriteOperation : OperationBase
    {
        public EnumWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {
        }

        public override StatementSyntax Generate()
        {
            return SF.IfStatement(
                condition: SF.BinaryExpression(SyntaxKind.EqualsExpression, Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.ToString())),
                statement: SF.Block(
                    SF.ExpressionStatement(SG.WriteString(Basics.GenerateReadOnlySpanNameIdentifier(ClassSymbol,MemberDecl)))));
        }
    }
}
