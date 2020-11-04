using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.ClassDeclarations
{
    internal class EnumClassDeclaration : SimpleClassDeclaration
    {
        public EnumClassDeclaration(ClassDeclMeta classdecl) : base(classdecl)
        {
        }
        public override MethodDeclarationSyntax DeclareTryParseMethod()
        {
            return new EnumTryParseMethodDeclaration(this).DeclareMethod();
        }
    }
}