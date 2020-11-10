using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.ReadWrite;
using System.Linq;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Operations
{
    internal class SimpleWriteOperation : OperationBase
    {
        public SimpleWriteOperation(INamedTypeSymbol classsymbol, MemberDeclarationMeta memberdecl) : base(classsymbol, memberdecl)
        {

        }
        public bool HaveSHITAttribute(out ExpressionSyntax shit)
        {
            shit = default;
            if (MemberDecl.DeclSymbol.GetAttributes().Length == 0)
            {
                return false;
            }
            foreach(var attr in MemberDecl.DeclSymbol.GetAttributes())
            {
                if (attr.AttributeClass.ToString().Equals("MongoDB.Client.Bson.Serialization.Attributes.BsonWriteIgnoreIf"))
                {
                    shit = SF.ParseExpression((string)attr.ConstructorArguments[0].Value);
                    
                    foreach (var meta in OperationBase.meta.Where(decl => decl.FullName.Equals(ClassSymbol.ToString())))
                    {
                        foreach (var member in meta.MemberDeclarations)
                        {
                            var id = SF.IdentifierName(member.DeclSymbol.Name);
                            var newid = SF.IdentifierName($"{Basics.WriteInputInVariableStringName}.{member.DeclSymbol.Name}");
                            foreach (var node in shit.DescendantNodes())
                            {
                                if (node.ToString().Equals(member.DeclSymbol.Name))
                                {
                                    if (node is ArgumentSyntax arg)
                                    {
                                        var newarg = SF.Argument(newid);
                                        shit = shit.ReplaceNode(node, newarg);
                                    }
                                    else
                                    {
                                        shit = shit.ReplaceNode(node, newid);
                                    }
                                }
                            }
                        }
                    }
                    shit = SF.ParenthesizedExpression(shit);
                    return true;
                }
            }
            return false;
        }
        public override StatementSyntax Generate()
        {
            //return SF.ExpressionStatement(SF.ParseExpression($"writer.WriteTypeNameValue({Basics.GenerateReadOnlySpanName(ClassSymbol, MemberDecl)}, message.{MemberDecl.DeclSymbol.Name})"));
            //return SF.ExpressionStatement(
            //            SF.InvocationExpression(
            //                Basics.SimpleMemberAccess(Basics.WriterInputVariableIdentifierName, SF.IdentifierName("Write_Type_Name_Value")),
            //                SF.ArgumentList()
            //                    .AddArguments(
            //                        SF.Argument(SF.IdentifierName(Basics.GenerateReadOnlySpanName(ClassSymbol, MemberDecl))),
            //                        SF.Argument(Basics.SimpleMemberAccess(Basics.WriteInputInVariableIdentifierName, SF.IdentifierName(MemberDecl.DeclSymbol.Name))))
            //            ));
            ITypeSymbol type = MemberDecl.DeclType;
            if (MemberDecl.DeclType.Name.Equals("Nullable"))
            {
                type = MemberDecl.DeclType.TypeArguments[0];
            }
            TypeMap.TryGetValue(type/*MemberDecl.DeclType*/, out var writeOp);
            if ( HaveSHITAttribute(out var expr))
            {                
                return SyntaxFactory.IfStatement(SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, expr), SyntaxFactory.Block(writeOp.GenerateWrite(ClassSymbol, MemberDecl)));
            }
            else
            {
                return writeOp.GenerateWrite(ClassSymbol, MemberDecl);
            }
            

        }
    }
}
