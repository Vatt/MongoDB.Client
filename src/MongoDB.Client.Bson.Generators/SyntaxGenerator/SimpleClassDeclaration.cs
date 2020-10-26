using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods;
using System;
using System.Collections.Generic;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator
{
    internal class SimpleClassDeclaration : ClassDeclarationBase
    {
        public SimpleClassDeclaration(ClassDeclMeta classdecl) : base(classdecl)
        {

        }



        public override MethodDeclarationSyntax DeclareTryParseMethod()
        {
            return new SimpleTryParseMethodDeclaration(this).DeclareTryParseMethod();
        }

        public override TypeArgumentListSyntax GetInterfaceParameters()
        {
            return SF.TypeArgumentList().AddArguments(SF.ParseTypeName(ClassSymbol.Name));
        }

        public override TypeSyntax GetTryParseMethodOutParameter()
        {
            return SF.ParseTypeName(ClassSymbol.Name);
        }
        public override ClassDeclarationSyntax Build()
        {
            var decl = SF.ClassDeclaration(GeneratorBasics.GenerateSerializerName(ClassSymbol));
            return decl.WithBaseList(SF.BaseList(GetBaseList()))
                       .WithMembers(GenerateStaticNamesSpans())
                       .AddMembers(DeclareTryParseMethod());
        }
    }
}
