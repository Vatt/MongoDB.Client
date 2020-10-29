using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System;
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
            return SF.ExpressionStatement( 
                    SF.InvocationExpression(
                               expression: SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                Basics.SimpleMemberAccess(serializer, writeMethod),
                                                SF.IdentifierName("Write")),
                               argumentList: SF.ArgumentList()
                                                 .AddArguments(
                                                     SF.Argument( default, SF.Token(SyntaxKind.RefKeyword), Basics.WriterInputVariableIdentifierName),
                                                     SF.Argument(Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.Name))))));

        }
    }
}
