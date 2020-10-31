using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class SimpleWriteOperation : OperationBase
    {
        public SimpleWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        public override StatementSyntax Generate()
        {
            //return SF.ExpressionStatement(SF.ParseExpression($"writer.WriteTypeNameValue({Basics.GenerateReadOnlySpanName(ClassSymbol, MemberDecl)}, message.{MemberDecl.DeclSymbol.Name})"));
            return SF.ExpressionStatement(
                        SF.InvocationExpression(
                            Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("Write_Type_Name_Value")),
                            SF.ArgumentList()
                                .AddArguments(
                                    SF.Argument(SF.IdentifierName(Basics.GenerateReadOnlySpanName(ClassSymbol, MemberDecl))),
                                    SF.Argument(Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.Name))))
                        ));

        }
    }
}
