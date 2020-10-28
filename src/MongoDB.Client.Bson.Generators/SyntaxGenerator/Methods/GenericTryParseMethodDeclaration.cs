using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MongoDB.Client.Bson.Generators.SyntaxGenerator.Core;
using System;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace MongoDB.Client.Bson.Generators.SyntaxGenerator.Methods
{
    class GenericTryParseMethodDeclaration : TryParseMethodDeclatationBase
    {
        public GenericTryParseMethodDeclaration(ClassDeclarationBase classdecl) : base(classdecl)
        {

        }
        public SeparatedSyntaxList<TypeSyntax> GetGenericParametersList()
        {
            var genericsParameters = new SeparatedSyntaxList<TypeSyntax>();
            for (var index = 0; index < ClassSymbol.TypeArguments.Length; index++)
            {
                genericsParameters = genericsParameters.Add(SF.ParseTypeName(ClassSymbol.TypeArguments[index].Name));
            }
            return genericsParameters;
        }
        public override ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier()
        {
            return SF.ExplicitInterfaceSpecifier(
                  SF.GenericName(Basics.SerializerInterfaceIdentifier, SF.TypeArgumentList(
                      new SeparatedSyntaxList<TypeSyntax>()
                          .Add(SF.GenericName(SF.ParseToken(ClassSymbol.Name), SF.TypeArgumentList(GetGenericParametersList())))
                      )),
                  SF.Token(SyntaxKind.DotToken));
        }

        public override TypeSyntax GetParseMethodOutParameter()
        {
            return SF.GenericName(SF.ParseToken(ClassSymbol.Name), SF.TypeArgumentList(GetGenericParametersList()));
        }

        public override BlockSyntax GenerateMethodBody()
        {
            throw new NotImplementedException();
        }
    }
}
