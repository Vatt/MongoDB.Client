using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.ClassDeclarations
{
    internal class SimpleClassDeclaration : ClassDeclarationBase
    {
        public SimpleClassDeclaration(ClassDeclMeta classdecl) : base(classdecl)
        {

        }



        public override MethodDeclarationSyntax DeclareTryParseMethod()
        {
            return new SimpleTryParseMethodDeclaration(this).DeclareMethod();
        }
        public override MethodDeclarationSyntax DeclareWriteMethod()
        {
            return new SimpleWriteMethodDeclaration(this).DeclareMethod();
        }
        public override TypeArgumentListSyntax GetInterfaceParameters()
        {
            return SF.TypeArgumentList().AddArguments(SF.ParseTypeName(ClassDecl.FullName));
        }

        public override TypeSyntax GetTryParseMethodOutParameter()
        {
            return SF.ParseTypeName(ClassSymbol.ToString());
        }
        public override ClassDeclarationSyntax Generate()
        {
            var decl = SF.ClassDeclaration(Basics.GenerateSerializerName(ClassSymbol));
            return decl.WithBaseList(SF.BaseList(GetBaseList()))
                       .WithMembers(GenerateStaticNamesSpans())
                       .AddMembers(DeclareTryParseMethod())
                       .AddMembers(DeclareWriteMethod());
        }


    }
}
