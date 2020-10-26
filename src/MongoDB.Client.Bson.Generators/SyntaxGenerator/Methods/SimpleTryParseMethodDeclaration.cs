using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System;
using System.Collections.Generic;
using System.Text;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    internal class SimpleTryParseMethodDeclaration : TryParseMethodDeclatationBase
    {
        public SimpleTryParseMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl)
        {

        }
        public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier()
        {
            return SF.ExplicitInterfaceSpecifier(
                   SF.GenericName(
                       GeneratorBasics.SerializerInterfaceIdentifier,
                       SF.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>().Add(SF.ParseTypeName(ClassSymbol.Name)))),
                   SF.Token(SyntaxKind.DotToken));
        }

        public override TypeSyntax GetParseMethodOutParameter() => SF.ParseTypeName(ClassSymbol.Name);
        //public ParameterListSyntax GetTryParseParameterList()
        //{
        //    return SF.ParameterList().AddParameters(SF.Parameter(SF.Identifier(ClassSymbol.Name)));
        //}
    }
}
